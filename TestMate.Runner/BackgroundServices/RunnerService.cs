using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Appium.Service;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using TestMate.Common.Enums;
using TestMate.Common.Models.Devices;
using TestMate.Common.Models.TestRequests;
using TestMate.Common.Models.TestRuns;
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

        string TestResultsWorkingPath = "C:\\Users\\lydin.camilleri\\Desktop\\Master's Code Repo\\TestMate\\TestMate.Runner\\Logs\\NUnit_TestResults\\";
        int TestRunRetryLimit = 3;
        int TestRunRetryDelay = 300000; //5 minutes 

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
                try
                {
                    _logger.LogInformation("Initiating Message Consumption Event");

                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation("Message Content: " + message);

                    TestRun? testRun = JsonConvert.DeserializeObject<TestRun>(message);

                    // Run the Appium tests in a separate thread
                    var thread = new Thread(async () =>
                    {
                        if (await ProcessTestRun(testRun, cancellationToken) == false)
                        {
                            _logger.LogWarning($"No available device found to serve TestRun {testRun.Id}!");

                            if (testRun.RetryCount < TestRunRetryLimit)
                            {
                                testRun.incrementRetryCount();
                                await incrementTestRunRetryCount(testRun);

                                //republish updated test run with a delay header
                                string message = JsonConvert.SerializeObject(testRun);
                                _logger.LogInformation($"Re-publishing message: {message} ");
                                var properties = _channel.CreateBasicProperties();
                                properties.Headers = new Dictionary<string, object>();
                                properties.Headers.Add("x-delay", TestRunRetryDelay); 

                                // Publish the message to the queue
                                Byte[] body = Encoding.UTF8.GetBytes(message);
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
                                _logger.LogError($"Failing to serve test run after {TestRunRetryLimit} attempts.");
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
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message!");
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                }

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
            ChangeStreamOptions options = new ChangeStreamOptions();
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
                            string message = JsonConvert.SerializeObject(testRun);
                            _logger.LogInformation($"Publishing message: {message} to exchange '{testRunExchange}'");

                            // Publish the message to the queue
                            Byte[] body = Encoding.UTF8.GetBytes(message);
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
            _logger.LogInformation("Triggering process of servicing Test Run " + testRun.Id.ToString());

            FilterDefinition<Device> filter = buildDeviceSelectionFilter(testRun.DeviceFilter);

            Device? device = null;
            List<Device> matchingDevices = _devicesCollection.Find(filter).ToList();
            if (matchingDevices != null)
            {
                foreach (Device matchingDevice in matchingDevices)
                {
                    //Get current state of properties and update device properties in DB
                    DeviceProperties properties = ConnectivityUtil.GetDeviceProperties(matchingDevice.IP, matchingDevice.TcpIpPort);
                    await updateDeviceProperties(matchingDevice, properties);

                    //Validate if the updated properties still match to confirm that filter is still satisfied
                    if (ValidateDeviceProperties(properties, JsonConvert.SerializeObject(testRun.DeviceFilter))) {
                        device = matchingDevice;
                        break;
                    }
                };

                if (device != null)
                {
                    AppiumLocalService appiumService = null;
                    try
                    {
                        await updateDeviceStatus(device, DeviceStatus.Running);
                        await updateTestRunStatus(testRun, TestRunStatus.Processing);

                        string workingFolder = TestResultsWorkingPath + testRun.TestRequestID.ToString() + "\\" + testRun.Id;

                        appiumService = new AppiumServiceBuilder()
                        .WithIPAddress("127.0.0.1")
                        .UsingAnyFreePort()
                        //.WithTimeOut(TimeSpan.FromMinutes(5))
                        .WithLogFile(new FileInfo(Path.Combine(workingFolder, "AppiumServerLog.txt")))
                        .Build();

                        if (!SetDeviceContextConfigurations(device, testRun.ContextConfiguration))
                        {
                            throw new Exception($"Could not set Device Context Configuration {testRun.ContextConfiguration}");
                        } ;

                        string udid = $"{device.IP}:{device.TcpIpPort}";
                        string app = $@"{testRun.ApkPath}";
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

                        string arguments =  $"\"{testRun.TestExecutablePath}\"" +
                                            $" --work=\"{workingFolder}\"" +
                                            $" --testparam:AppiumServerUrl=\"{appiumServerUrl}\"" +
                                            $" --testparam:APP=\"" + app + "\"" +
                                            $" --testparam:UDID=\"" + udid + "\"" +
                                            $" --out=\"TestSolution_ConsoleOutput.txt\" " +
                                            $" --result=\"NUnitResult.xml\"";

                        File.WriteAllText(workingFolder + "\\NUnitConsole_Arguments.txt", arguments);

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
                        if (appiumService != null)
                        {
                            appiumService.Dispose();
                        }

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

        public bool SetDeviceContextConfigurations(Device device, Dictionary<string,string>? configuration) {

            bool result = true;
            if(configuration != null)
            {
                foreach (var config in configuration)
                {
                    if (config.Value != null)
                    {
                        _logger.LogInformation($"Setting {config.Key} of {device.SerialNumber} to {config.Value}");
                        switch (config.Key)
                        {
                            case "Bluetooth":
                                result = device.SetBluetooth(Boolean.Parse(config.Value));
                                break;
                            case "AirplaneMode":
                                result = device.SetAirplaneMode(Boolean.Parse(config.Value));
                                break;
                            case "Brightness":
                                result = device.SetBrightness(Boolean.Parse(config.Value));
                                break;
                            case "AutoRotate":
                                result = device.SetAutoRotateMode(Boolean.Parse(config.Value));
                                break;
                            case "Orientation":
                                result = device.SetOrientation(int.Parse(config.Value));
                                break;
                            case "Location":
                                result = device.SetLocation(Boolean.Parse(config.Value));
                                break;
                            case "Volume":
                                result = device.SetVolume(Boolean.Parse(config.Value));
                                break;
                            case "Flashlight":
                                throw new NotImplementedException("Flashlight");
                            default:
                                throw new Exception("Unknown Device Context Configuration property: " + config.Key);
                        }
                    }
                    if (result != true)
                    {
                        _logger.LogInformation($"Failed to set {config.Key} of {device.SerialNumber} to {config.Value}");
                        return result;
                    }
                    else 
                    {
                        _logger.LogInformation($"Successfully set {config.Key} of {device.SerialNumber} to {config.Value}");
                    }
                }
            }
            return result;
        }

        public static bool ValidateDeviceProperties(DeviceProperties deviceProperties, string deviceFilterJson)
        {
            var deviceFilter = JsonConvert.DeserializeObject<Dictionary<string, object>>(deviceFilterJson);

            foreach (var filterItem in deviceFilter)
            {
                var property = typeof(DeviceProperties).GetProperty(filterItem.Key);

                if (property == null)
                {
                    // The property does not exist in the DeviceProperties class, so it cannot be satisfied.
                    return false;
                }

                var propertyValue = property.GetValue(deviceProperties)?.ToString();

                if (propertyValue != filterItem.Value?.ToString())
                {
                    // The property value does not match the filter value, so it cannot be satisfied.
                    return false;
                }
            }

            // All the filter properties have been satisfied by the corresponding properties in the DeviceProperties object.
            return true;
        }

        public FilterDefinition<Device> buildDeviceSelectionFilter(Dictionary<string, string> deviceFilter){
            var builder = Builders<Device>.Filter;
            var filter = builder.Empty;
            filter &= Builders<Device>.Filter.Eq(d => d.Status, DeviceStatus.Connected);
            foreach (var property in deviceFilter)
            {
                string key = "DeviceProperties." + property.Key;
                string value = property.Value;
                if (property.Key == "AndroidVersion" || property.Key == "SdkVersion")
                {
                    filter &= Builders<Device>.Filter.Eq(key, int.Parse(value));
                }
                else if (property.Key == "ScreenResolution")
                {
                    ScreenResolution screenResolution = JsonConvert.DeserializeObject<ScreenResolution>(value);
                    filter &= Builders<Device>.Filter.Eq(key, screenResolution);
                }
                else if (property.Key == "Battery")
                {
                    Battery battery = JsonConvert.DeserializeObject<Battery>(value);
                    filter &= Builders<Device>.Filter.Eq(key, battery);
                }
                else
                {
                    filter &= Builders<Device>.Filter.Eq("DeviceProperties." + property.Key, property.Value);
                }
            }
            return filter;
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

        public async Task updateDeviceProperties(Device device, DeviceProperties properties) {
            var filter = Builders<Device>.Filter.Eq(x => x.SerialNumber, device.SerialNumber);
            var update = Builders<Device>.Update
                .Set(d => d.DeviceProperties, properties);
             
            await _devicesCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = false });
        }


    }
}
