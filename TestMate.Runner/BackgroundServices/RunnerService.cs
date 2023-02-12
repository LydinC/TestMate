using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Core;
using NUnit.Framework.Interfaces;
using NUnit.Util;
using NUnit.VisualStudio.TestAdapter;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Service;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.IO.Packaging;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Xml;
using System.Xml.Linq;
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
        //private readonly CancellationToken _cancellationToken;

        string exchangeName = "TestRequestExchange";
        string queueName = "test-requests";
        string routingKey = "testRequestKey";

        public RunnerService(ILogger<RunnerService> logger, IMongoDatabase database, IConnection connection, IModel channel, IConfiguration configuration)
        {
            _devicesCollection = database.GetCollection<Device>("Devices");
            _testRequestsCollection = database.GetCollection<TestRequest>("TestRequests");
            _logger = logger;
            _connection = connection;
            _channel = channel;
            _configuration = configuration;
            //_cancellationToken = cancellationToken;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {

            _logger.LogInformation("======= Starting RunnerService =======");

            try
            {
                _logger.LogInformation("Setting up RabbitMQ");
                //Exchange Declaration
                _channel.ExchangeDeclare(
                    exchange: exchangeName,
                    type: ExchangeType.Direct);
                _logger.LogInformation("RabbitMQ - TestRequestExchange declared");

                // Declare a queue for test request messages.
                _channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                _logger.LogInformation("RabbitMQ - 'test-requests' queue declared");

                //Bind queue to exchange
                _channel.QueueBind(
                    queue: queueName,
                    exchange: exchangeName,
                    routingKey: routingKey,
                    arguments: null);
                _logger.LogInformation("RabbitMQ - 'test-requests' queue binded with TestRequestExchange");

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
                TestRequest testRequest = DeserializeTestRequest(message);

                // Run the Appium tests in a separate thread
                var thread = new Thread(() => ProcessTestRequest(cancellationToken, testRequest));
                _logger.LogInformation("Setting up new thread to process message " + thread.Name);
                thread.Start();
                
                // Acknowledge the message
                _logger.LogInformation("Acknowledging Message ");
                _channel.BasicAck(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false);
            };

            //Start the consumer
            _channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);
            _logger.LogInformation("Consumer Started");


            /* LISTENER FOR PUBLISHER */
            _logger.LogInformation("Setting up Listener on MongoDB using ChangeStream");
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<TestRequest>>();
            var options = new ChangeStreamOptions();
            options.FullDocument = ChangeStreamFullDocumentOption.UpdateLookup;
            _logger.LogInformation("Successfully set up MongoDB ChangeStream");

            using (var changeStream = _testRequestsCollection.Watch(pipeline, options))
            {
                // Listen for change events
                await changeStream.ForEachAsync(async change =>
                {
                    // Check if the change is a new insert
                    if (change.OperationType == ChangeStreamOperationType.Insert)
                    {
                        // Publish the test request as a message to RabbitMQ Queue
                        var testRequest = change.FullDocument;
                        var message = SerializeTestRequest(testRequest);
                        _logger.LogInformation("Publishing message: {message} to exchange '{exchangeName}'", message, exchangeName);

                        // Publish the message to the queue
                        var body = Encoding.UTF8.GetBytes(message);
                        _channel.BasicPublish(
                            exchange: exchangeName,
                            routingKey: routingKey,
                            basicProperties: null,
                            body: body
                            );

                        _logger.LogInformation("Published successfully!");
                    }
                });
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _channel.Dispose();
            _channel.Close();
            _connection.Close();
            return base.StopAsync(cancellationToken);
        }

        static string SerializeTestRequest(TestRequest testRequest)
        {
            return JsonConvert.SerializeObject(testRequest);
        }

        static TestRequest DeserializeTestRequest(string message)
        {
            TestRequest testRequest = JsonConvert.DeserializeObject<TestRequest>(message);
            return testRequest;
        }

        private void ProcessTestRequest(CancellationToken cancellationToken, TestRequest testRequest)
        {
            _logger.LogInformation("Triggering process of servicing Test Request " + testRequest.RequestId.ToString());

            testRequest.TestRunConfiguration = new TestRunConfiguration
                (
                    applicationUnderTest: @"""C:\Users\lydin.camilleri\Desktop\Master's Code Repo\UPLOADS\44fb61fb-62ad-4a92-a562-d8d25fee21c1\Application Under Test\com.xlythe.calculator.material_93.apk""",
                    testSolutionPath: "\"C:\\Users\\lydin.camilleri\\Desktop\\Master's Code Repo\\Appium Tests\\AppiumTests\\bin\\Debug\\net7.0\\AppiumTests.dll\"",
                    desiredDeviceProperties: "{\"Model\": \"SM-G960F\", \"AndroidVersion\": \"> 10\" }",
                    constraints: new TestRunConstraints(),
                    contextConifguration: new ContextConifguration()
                 );
           
            JObject desiredDeviceProperties = JObject.Parse(testRequest.TestRunConfiguration.DesiredDeviceProperties);

            //var builder = Builders<Device>.Filter;
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

            // Create a config class which holds:
            // - Device selection parameters
            // - Context selection parameters
            // - Test run constrains (e.g. max devices, max contexts, max runtime)

            //var filter = Builders<Device>.Filter.And(
            //    Builders<Device>.Filter.Eq(d => d.Status, DeviceStatus.Connected),
            //    Builders<Device>.Filter.Eq(d => d.DeviceProperties.Model, "NE2213")
            //);
                

            FilterDefinition<Device> filter = "{\"DeviceProperties.SdkVersion\" : {$gte : 29}}";
            filter &= Builders<Device>.Filter.Eq(d => d.Status, DeviceStatus.Connected);

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
                    //update device status to running
                    var deviceFilter = Builders<Device>.Filter.Where(x => x.SerialNumber == device.SerialNumber);
                    var deviceUpdate = Builders<Device>.Update.Set(d => d.Status, DeviceStatus.Running);
                    var result = _devicesCollection.UpdateOneAsync(deviceFilter, deviceUpdate);

                    updateTestRequestStatus(testRequest.RequestId, TestRequestStatus.Processing);

                    var appiumService = new AppiumServiceBuilder()
                        .WithIPAddress("127.0.0.1")
                        .UsingAnyFreePort()
                        .WithLogFile(new FileInfo(Path.Combine(Directory.GetCurrentDirectory(),"Logs", "AppiumServerLogs", testRequest.RequestId.ToString()+ ".txt")))
                        .Build();
                    try
                    {
                        appiumService.Start();
                        string udid = $"{device.IP}:{device.TcpIpPort}";
                        string app = $"{testRequest.TestRunConfiguration.ApplicationUnderTest}";
                        string appiumServerUrl = $"{appiumService.ServiceUrl.AbsoluteUri}";
                        string fileName = @"C:\Program Files\NUnit.Console-3.16.2\bin\nunit3-console.exe";

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


                        string arguments = testRequest.TestRunConfiguration.TestSolutionPath +
                                            " --work=\"C:\\Users\\lydin.camilleri\\Desktop\\Master's Code Repo\\TestMate\\TestMate.Runner\\Logs\\NUnit_TestResults\\" + testRequest.RequestId + "\"" +
                                            " --testparam:AppiumServerUrl=" + appiumServerUrl +
                                            " --testparam:APP=" + app +
                                            " --testparam:UDID=" + udid +
                                            " --out=\"ConsoleOutput.txt\" " +
                                            " --result=\"NUnitResult.xml\"";

                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = fileName,
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
                            _logger.LogInformation("Process " + process.Id + " - Standard output: {0}", output);
                            _logger.LogInformation("Process " + process.Id + " - Standard error: {0}", error);


                            //TODO: Consider catering for different failure statuses as defined in https://docs.nunit.org/articles/nunit/running-tests/Console-Runner.html
                            if (process.ExitCode >= 0)
                            {
                                updateTestRequestStatus(testRequest.RequestId, TestRequestStatus.Completed);
                            }
                            else 
                            {
                                updateTestRequestStatus(testRequest.RequestId, TestRequestStatus.Failed);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[TEST RUN FAILED!] -- " + ex.StackTrace, ex);
                        updateTestRequestStatus(testRequest.RequestId, TestRequestStatus.Failed);
                    }
                    finally
                    {
                        //update device status to connected (idle)
                        deviceFilter = Builders<Device>.Filter.Where(x => x.SerialNumber == device.SerialNumber);
                        deviceUpdate = Builders<Device>.Update.Set(d => d.Status, DeviceStatus.Connected);
                        result = _devicesCollection.UpdateOneAsync(deviceFilter, deviceUpdate);

                        appiumService.Dispose();
                    }
                }
                else 
                {
                    //POSTPONE CONSUME OF QUEUE
                    _logger.LogWarning($"Postponing consumption of Test Request {testRequest.RequestId}");
                    //TODO:
                }

            }
            else 
            {
                //POSTPONE CONSUME OF QUEUE  
                _logger.LogWarning($"Postponing consumption of Test Request {testRequest.RequestId}");
                //TODO:
            }


            //Check if there is an available device at the moment to service the request

            //If ok,
            //Check if device is actually connected using ADB
            //Get Device Props again to make sure that they match the required props
            //IF OK    
            //Set status of device to Running
            //Start Appium Server
            //var appiumService = new AppiumServiceBuilder()
            //    .WithIPAddress("0.0.0.0")
            //    .UsingAnyFreePort()
            //    .WithLogFile(new FileInfo(string.Format("C:\\Users\\lydin.camilleri\\Desktop\\Master's Code Repo\\TestMate\\TestMate.Runner\\Logs\\NUnit_TestResults\\{0}", requestNumber)))
            //    .Build();

            //appiumService.Start();
            //Make sure that appium server is started
            //Build the appiumOptions required to be passed over to the test runner executable
            //Example: UDID of device selected, appium server url, applicationPackage name, etc)


            //ELSE
            //Try to get another device otherwise if not available postpone the request

            //If not, republish the message to be consumed at a later time


        }




        public async void updateTestRequestStatus(Guid requestId, TestRequestStatus status)
        {
            var testRequestFilter = Builders<TestRequest>.Filter.Where(x => x.RequestId == requestId);
            var testRequestUpdate = Builders<TestRequest>.Update.Set(x => x.Status, status);
            await _testRequestsCollection.UpdateOneAsync(testRequestFilter, testRequestUpdate);
        }


    }





}
