using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Appium.Service;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using TestMate.Common.Enums;
using TestMate.Common.Models.Devices;
using TestMate.Common.Models.TestRequests;
using TestMate.Common.Utils;

namespace TestMate.Runner.BackgroundServices
{
    class RunnerService : BackgroundService
    {
        private readonly ILogger<RunnerService> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<Device> _devicesCollection;
        private readonly IMongoCollection<TestRequest> _testRequestsCollection;
        private readonly IMongoCollection<TestRun> _testRunsCollection;
        //private readonly CancellationToken _cancellationToken;

        string testRunExchange = "TestRunExchange";
        string testRunQueue = "test-run-queue";
        string testRunRoutingKey = "testRunRoutingKey";
      
        public RunnerService(ILogger<RunnerService> logger, IMongoDatabase database, IConnection connection, IModel channel, IConfiguration configuration)
        {
            _devicesCollection = database.GetCollection<Device>("Devices");
            _testRequestsCollection = database.GetCollection<TestRequest>("TestRequests");
            _testRunsCollection = database.GetCollection<TestRun>("TestRuns");

            _logger = logger;
            _connection = connection;
            _channel = channel;
            _configuration = configuration;
            //_cancellationToken = cancellationToken;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //--https://www.c-sharpcorner.com/article/rabbitmq-retry-architecture/

            try
            {
                _logger.LogInformation("======= Starting RunnerService =======");
                _logger.LogInformation("Setting up RabbitMQ");

                //Exchange Declarations 
                Dictionary<string, object> args = new Dictionary<string, object>();
                args.Add("x-delayed-type", "direct");
                _channel.ExchangeDeclare(exchange: testRunExchange, type: "x-delayed-message", true, false, args);
                _logger.LogInformation("RabbitMQ - TestRunExchange declared");

                //Queues Declarations
                _channel.QueueDeclare(queue: testRunQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                _logger.LogInformation("RabbitMQ - TestRunQueue declared");

                //Binding Declarations
                _channel.QueueBind(queue: testRunQueue, exchange: testRunExchange, routingKey: testRunRoutingKey);
                _logger.LogInformation("RabbitMQ - TestRunQueue binded with TestRunExchange");

                //Setup Qos
                _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

                _logger.LogInformation("Successfully set up RabbitMQ");
                _logger.LogInformation("======= RunnerService Setup Complete =======");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogInformation("======= RunnerService Setup Failed =======");
            }


            /* CONSUMER */
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                _logger.LogInformation("Consumer Received Message!");

                // Get the message body
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Message received: " + message);
                TestRun testRun = JsonConvert.DeserializeObject<TestRun>(message);


                // Run the Appium tests in a separate thread
                var thread = new Thread(async () =>
                {

                    if (await ProcessTestRun(testRun, cancellationToken) == false)
                    {
                        _logger.LogWarning("No available device found!");

                        testRun.incrementRetryCount();
                        await incrementTestRunRetryCount(testRun);

                        if (testRun.RetryCount <= 3)
                        {
                            //republish updated test run with a delay header
                            var message = JsonConvert.SerializeObject(testRun);
                            _logger.LogInformation($"Re-publishing message: {message} ");
                            var properties = _channel.CreateBasicProperties();
                            properties.Headers = new Dictionary<string, object>();
                            properties.Headers.Add("x-delay", 300000); //5 minutes
                            // Publish the message to the queue
                            var body = Encoding.UTF8.GetBytes(message);
                            _channel.BasicPublish(
                                exchange: testRunExchange,
                                routingKey: testRunRoutingKey,
                                basicProperties: properties,
                                body: body
                                );
                            _logger.LogInformation("Re-published successfully!");
                        }
                        else
                        {
                            _logger.LogError("Failing to serve test run after 3 attempts.");
                            await updateTestRunStatus(testRun, TestRunStatus.FailedNoDevices);
                        }
                    }

                    //Always acnowledge message. If request was not processed, another message was published with the respective delay
                    _logger.LogInformation("Acknowledging Message");
                    _channel.BasicAck(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false);
                });

                _logger.LogDebug("Starting New Thread - " + thread.ManagedThreadId);
                thread.Start();

            };

            //Start the consumer
            _channel.BasicConsume(
                queue: testRunQueue,
                autoAck: false,
                consumer: consumer);
            _logger.LogInformation("Consumer Started");

            /* LISTENER FOR PUBLISHER */
            _logger.LogInformation("Setting up Listener on MongoDB using ChangeStream");
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<TestRun>>();
            var options = new ChangeStreamOptions();
            options.FullDocument = ChangeStreamFullDocumentOption.UpdateLookup;
            _logger.LogInformation("Successfully set up MongoDB ChangeStream");

            using (var changeStream = _testRunsCollection.Watch(pipeline, options))
            {
                // Listen for change events
                await changeStream.ForEachAsync(async change =>
                {
                    try
                    {
                        // Check if the change is a new insert
                        if (change.OperationType == ChangeStreamOperationType.Insert)
                        {
                            // Publish the test request as a message to RabbitMQ Queue
                            var testRun = change.FullDocument;
                            var message = JsonConvert.SerializeObject(testRun);
                            _logger.LogInformation($"Publishing message: {message} to exchange '{testRunExchange}'");

                            // Publish the message to the queue
                            var body = Encoding.UTF8.GetBytes(message);
                            _channel.BasicPublish(
                                exchange: testRunExchange,
                                routingKey: testRunRoutingKey,
                                basicProperties: null,
                                body: body
                                );

                            _logger.LogInformation("Published successfully!");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to publish Test Run document {change.DocumentKey} in queue! -> " + ex.Message);
                    }
                });
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _channel.Close();
            _connection.Close();
            return base.StopAsync(cancellationToken);
        }

        private async Task<bool> ProcessTestRun(TestRun testRun, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Triggering process of servicing Test Run " + testRun.Id.ToString());

            testRun.TestSolutionPath = "\"C:\\Users\\lydin.camilleri\\Desktop\\Master's Code Repo\\Appium Tests\\AppiumTests\\bin\\Debug\\net7.0\\AppiumTests.dll\"";
            testRun.ApplicationUnderTest = @"""C:\Users\lydin.camilleri\Desktop\Master's Code Repo\UPLOADS\44fb61fb-62ad-4a92-a562-d8d25fee21c1\Application Under Test\com.xlythe.calculator.material_93.apk""";
            
            var builder = Builders<Device>.Filter;
            var filter = builder.Empty;
            filter &= Builders<Device>.Filter.Eq(d => d.Status, DeviceStatus.Connected);
            foreach(var property in testRun.DeviceFilter)
            {
               //TODO: FIX THIS FILTER TO COVER FOR Device.DEVICEPROPERTIES and then the key
                filter &= Builders<Device>.Filter.Eq(property.Key, property.Value);
            }

            //JObject desiredDeviceProperties = JObject.Parse(testRun.DesiredDeviceProperties);


            /* CODE COMMENTED FOR GENERATING MONGO FILTER
             * 
             * var builder = Builders<Device>.Filter;
            //var filter = builder.Empty;


            //foreach (var property in desiredDeviceProperties)
            //{
            //    string key = property.Key.ToString();
            //    var value = property.Value;

            //    switch (value.Type)
            //    {
            //        case JTokenType.String:
            //            string stringValue = value.ToString();
            //            Regex operatorRegex = new Regex("^(<=|>=|<|>|=)\\s*\\d+(\\.\\d+)?$");

            //            if (operatorRegex.IsMatch(stringValue)) {
            //                string[] tokenizedOperator = stringValue.Split(" ");

            //                switch (tokenizedOperator[0])
            //                {
            //                    case ">=":
            //                        filter = filter & Builders<Device>.Filter.Eq(d => d.DeviceProperties., DeviceStatus.Connected);
            //                        break;
            //                    case ">":
            //                        filter &= builder.AnyGt(d => d.DeviceProperties[key.ToString()], tokenizedOperator[1]);
            //                        break;

            //                    case "<=":
            //                        filter = filter & builder.AnyLte(d => d.DeviceProperties[key], tokenizedOperator[1]);
            //                        break;

            //                    case "<":
            //                        filter = filter & builder.AnyLt(d => d.DeviceProperties[key], tokenizedOperator[1]);
            //                        break;

            //                    case "=":
            //                        filter = filter & builder.AnyEq(key, tokenizedOperator[1]);
            //                        break;
            //                }
            //            } else 
            //            {
            //                filter = filter & builder.Eq(key, stringValue);

            //            }

            //            break;
            //        case JTokenType.Array:
            //            filter = filter & builder.In(key, value.ToObject<string[]>());
            //            break;
            //    }
            //}
            */

            // Create a config class which holds:
            // - Device selection parameters
            // - Context selection parameters
            // - Test run constrains (e.g. max devices, max contexts, max runtime)

            //var filter = Builders<Device>.Filter.And(
            //    Builders<Device>.Filter.Eq(d => d.Status, DeviceStatus.Connected),
            //    Builders<Device>.Filter.Eq(d => d.DeviceProperties.Model, "NE2213")
            //);


            //FilterDefinition<Device> filter = "{\"DeviceProperties.SdkVersion\" : {$gte : 29}}";
            //filter &= Builders<Device>.Filter.Eq(d => d.Status, DeviceStatus.Connected);

            Device? device = null;

            var matchingDevices = _devicesCollection.Find(filter).ToList();

            if (matchingDevices != null)
            {
                foreach(Device matchingDevice in matchingDevices)
                {
                    DeviceProperties actualDeviceProperties = ConnectivityUtil.GetDeviceProperties(matchingDevice.IP, matchingDevice.TcpIpPort);
                    //TODO: Validate actualDeviceProperties against the required properties
                    //for now assuming valid
                    if(true){
                        device = matchingDevice;
                        break;
                    }
                };

                if (device != null)
                {
                    string workingFolder = "C:\\Users\\lydin.camilleri\\Desktop\\Master's Code Repo\\TestMate\\TestMate.Runner\\Logs\\NUnit_TestResults\\" + testRun.TestRequestID.ToString() + "\\" + testRun.Id;
                    var appiumService = new AppiumServiceBuilder()
                        .WithIPAddress("127.0.0.1")
                        .UsingAnyFreePort()
                        .WithLogFile(new FileInfo(Path.Combine(workingFolder, "AppiumServerLog.txt")))
                        .Build();

                  
                    try
                    {
                        await updateDeviceStatus(device, DeviceStatus.Running);
                        await updateTestRunStatus(testRun, TestRunStatus.Processing);

                        string udid = $"{device.IP}:{device.TcpIpPort}";
                        string app = $"{testRun.ApplicationUnderTest}";
                        string nUnitConsolePath = @"C:\Program Files\NUnit.Console-3.16.2\bin\nunit3-console.exe";
                        string appiumServerUrl = $"{appiumService.ServiceUrl.AbsoluteUri}";

                        appiumService.Start();
                        
                        /* RUNNING VIA NUNIT TESTPACKAGE TECHNIQUE 
                        // Initialize the test engine
                        ITestEngine engine = TestEngineActivator.CreateInstance();
                        string dllPath = @"C:\Users\lydin.camilleri\Desktop\Master's Code Repo\Appium Tests\AppiumTests\bin\Debug\net7.0\AppiumTests.dll";
                        TestPackage testPackage = new TestPackage(dllPath);
                        ITestRunner runner = engine.GetRunner(testPackage);
                        XmlNode testResult = runner.Run(listener: null, TestFilter.Empty);
                        string TestResultString = XElement.Parse(testResult.OuterXml).ToString();
                        _logger.LogInformation(TestResultString);
                        File.WriteAllText(@"path\to\output\file.xml", TestResultString);
                        
                        //Using RemoteTestRunner : Issue could not load Assemblies (dll's)
                        TestPackage testPackage = new TestPackage(@"C:\Users\lydin.camilleri\Desktop\Master's Code Repo\Appium Tests\AppiumTests\bin\Debug\net7.0\AppiumTests.dll");
                        RemoteTestRunner remoteTestRunner = new RemoteTestRunner();
                        remoteTestRunner.Load(testPackage);
                        TestResult testResult = remoteTestRunner.Run(listener: null, TestFilter.Empty, true, LoggingThreshold.All);
                        */

                        string arguments = testRun.TestSolutionPath +
                                            " --work=\"" + workingFolder + "\"" +
                                            " --testparam:AppiumServerUrl=" + appiumServerUrl +
                                            " --testparam:APP=" + app +
                                            " --testparam:UDID=" + udid +
                                            " --out=\"DllOutput.txt\" " +
                                            " --result=\"NUnitResult.xml\"";

                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = nUnitConsolePath,
                            Arguments = arguments,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                        };
                        
                        using (Process process = Process.Start(startInfo))
                        {
                            string output = process.StandardOutput.ReadToEnd();
                            string error = process.StandardError.ReadToEnd();

                            process.WaitForExit();
                            _logger.LogInformation("Process " + process.Id + " exited with code " + process.ExitCode);
                            File.WriteAllText(workingFolder + "\\NUnitConsole_StandardOutput.txt", output);
                            File.WriteAllText(workingFolder + "\\NUnitConsole_StandardError.txt", error);

                            //TODO: Consider catering for different failure statuses as defined in https://docs.nunit.org/articles/nunit/running-tests/Console-Runner.html
                            if (process.ExitCode >= 0)
                            {
                                await updateTestRunStatus(testRun, TestRunStatus.Completed);
                            }
                            else 
                            {
                                await updateTestRunStatus(testRun, TestRunStatus.Failed);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[TEST RUN FAILED!] - " + ex.Message + ex.StackTrace, ex);
                        await updateTestRunStatus(testRun, TestRunStatus.Failed);
                    }
                    finally
                    {
                        await updateDeviceStatus(device, DeviceStatus.Connected);
                        appiumService.Dispose();
                        
                    }
                    return true;
                }
                else 
                {
                    _logger.LogWarning($"Postponing consumption of Test Run {testRun.Id}");
                    return false;
                }

            }
            else 
            {
                _logger.LogWarning($"Postponing consumption of Test Run {testRun.Id}");
                return false;
            }
        }




        public async Task updateTestRunStatus(TestRun testRun, TestRunStatus status)
        {
            var testRunFilter = Builders<TestRun>.Filter.Where(x => x.Id == testRun.Id);
            var testRunUpdate = Builders<TestRun>.Update.Set(x => x.Status, status);
            await _testRunsCollection.UpdateOneAsync(testRunFilter, testRunUpdate);
        }

        public async Task incrementTestRunRetryCount(TestRun testRun)
        {
            var testRunFilter = Builders<TestRun>.Filter.Where(x => x.Id == testRun.Id);
            var testRunUpdate = Builders<TestRun>.Update.Inc(x => x.RetryCount, 1);
            await _testRunsCollection.UpdateOneAsync(testRunFilter, testRunUpdate);
        }

        public async Task updateDeviceStatus(Device device, DeviceStatus status) {
            var deviceFilter = Builders<Device>.Filter.Where(d => d.SerialNumber == device.SerialNumber);
            var deviceUpdate = Builders<Device>.Update.Set(d => d.Status, status);
            await _devicesCollection.UpdateOneAsync(deviceFilter, deviceUpdate);
        }
    }





}
