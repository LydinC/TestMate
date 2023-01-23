using MongoDB.Driver;
using Newtonsoft.Json;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Service;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TestMate.Common.Models.TestRequests;

namespace TestMate.Runner.BackgroundServices
{
    class RunnerService : BackgroundService
    {
        private readonly ILogger<RunnerService> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IConfiguration _configuration;
        //private readonly CancellationToken _cancellationToken;


        string exchangeName = "TestRequestExchange";
        string queueName = "test-requests";
        string routingKey = "testRequestKey";


        public RunnerService(ILogger<RunnerService> logger, IConnection connection, IModel channel, IConfiguration configuration)
        {
            _logger = logger;
            _connection = connection;
            _channel = channel;
            _configuration = configuration;
            //_cancellationToken = cancellationToken;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {

            _logger.LogInformation("Starting RunnerService");

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
                _logger.LogInformation("Successfully set up RabbitMQ");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }


            /* CONSUMER */
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                _logger.LogInformation("Consumer Received Message!");

                // Get the message body
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Process the message
                _logger.LogInformation("Message received: " + message);

                // Deserialize the message into a TestRequest object
                TestRequest testRequest = DeserializeTestRequest(message);

                int availablePort = FindAvailablePort;

                // Run the Appium tests in a separate thread
                var thread = new Thread(() => ProcessTestRequest(cancellationToken, testRequest, availablePort));
                _logger.LogInformation("Setting up new thread to process message " + thread.Name);
                thread.Start();

                // Acknowledge the message
                _logger.LogInformation("Acknowledging Message");
                _channel.BasicAck(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false);
            };

            //Start the consumer
            _channel.BasicConsume(
                queue: queueName,
                autoAck: false,
                consumer: consumer);

            /* LISTENER FOR PUBLISHER */
            _logger.LogInformation("Setting up Listener on MongoDB using ChangeStream");
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            var database = mongoClient.GetDatabase("testframework_db");
            var collection = database.GetCollection<TestRequest>("TestRequests");
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<TestRequest>>();
            var options = new ChangeStreamOptions();
            options.FullDocument = ChangeStreamFullDocumentOption.UpdateLookup;
            _logger.LogInformation("Successfully set up MongoDB ChangeStream");

            using (var changeStream = collection.Watch(pipeline, options))
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



            ////CONSUMER


            //consumer.Received += async (model, ea) =>
            //{
            //    var body = ea.Body.ToArray();
            //    string message = Encoding.UTF8.GetString(body);
            //    Console.WriteLine("Received message: {0}", message);

            //    // Deserialize the message into a TestRequest object
            //    TestRequest testRequest = DeserializeTestRequest(message);

            //    // Set up the Appium connection and execute the tests
            //    /*var appiumOptions = new AppiumOptions();
            //    appiumOptions.AddAdditionalCapability("platformName", testRequest.PlatformName);
            //    appiumOptions.AddAdditionalCapability("deviceName", testRequest.DeviceName);
            //    appiumOptions.AddAdditionalCapability("app", testRequest.AppPath);

            //    using var appiumDriver = new AndroidDriver<AndroidElement>(new Uri("http://localhost:4723/wd/hub"), appiumOptions);
            //    var testResult = await ExecuteTests(appiumDriver);

            //    // Publish the test result to the "appiumresults" exchange
            //    var testResultJson = JsonConvert.SerializeObject(testResult);
            //    var testResultBody = Encoding.UTF8.GetBytes(testResultJson);
            //    _channel.BasicPublish("appiumresults", "", null, testResultBody);
            //    Console.WriteLine("Sent message: {0}", testResultJson);
            //    */

            //    //ACKNOWLEDGEMENTS?????
            //    // Acknowledge the message
            //    _channel.BasicAck(ea.DeliveryTag, multiple: false);

            //};
            //_channel.BasicConsume(queueName, true, consumer);
            //// Start the consumer in a separate task
            //var consumerTask = Task.Factory.StartNew(() =>
            //{
            //    _channel.BasicConsume(queueName, true, consumer);
            //}, stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        }

        static string SerializeTestRequest(TestRequest testRequest)
        {
            // Serialize the test request object to a string
            return JsonConvert.SerializeObject(testRequest);
        }

        static TestRequest DeserializeTestRequest(string message)
        {
            // Serialize the test request object to a string
            return JsonConvert.DeserializeObject<TestRequest>(message);
        }


        private int FindAvailablePort
        {
            get
            {
                //TODO: might require using .AnyFreePort in appium itself?
                // Finding an available port by trying to bind a socket to a series of ports and seeing which ones are available
                _logger.LogInformation("Searching for an available port ... ");

                var port = 4723; // Appium default port
                while (true)
                {
                    try
                    {
                        using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                        {
                            socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
                            _logger.LogInformation("Available Port Identified: " + port);
                            return port;
                        }
                    }
                    catch (SocketException)
                    {
                        port++;
                    }
                }
            }
        }

        private void ProcessTestRequest(CancellationToken cancellationToken, TestRequest testRequest, int port)
        {

            // Set up the Appium connection and run the tests on the specified port
            _logger.LogInformation("Running Appium tests on port " + port);

            //Extract neccessary information from TestRequest
            string requestNumber = testRequest.RequestId.ToString();
            string testSolutionPath = testRequest.TestSolutionPath;
            string appiumOptionsJSON = "{\"platformName\":\"iOS\",\"deviceName\":\"iPhone 8\"}"; //testRequest.AppiumOptions;

            try
            {
                AppiumOptions appiumOptions = JsonConvert.DeserializeObject<AppiumOptions>(appiumOptionsJSON);
                _logger.LogInformation($"AppiumOptions JSON of TestRequest {testRequest.Id} is valid!");
            }
            catch (JsonReaderException e)
            {
                _logger.LogError($"AppiumOptions of TestRequest {testRequest.Id} is not valid! Error: {e.Message}");
            }

            //Start Appium Server
            var appiumService = new AppiumServiceBuilder()
                .WithIPAddress("0.0.0.0")
                .UsingPort(port)
                .WithLogFile(new FileInfo(string.Format("C:\\Users\\lydin.camilleri\\Desktop\\Master's Code Repo\\TestMate\\TestMate.Runner\\Logs\\NUnit_TestResults\\{0}", requestNumber)))
                .Build();

            appiumService.Start();



            //    using var appiumDriver = new AndroidDriver<AndroidElement>(new Uri("http://localhost:4723/wd/hub"), appiumOptions);
            //    var testResult = await ExecuteTests(appiumDriver);


            string fileName = @"C:\Program Files\NUnit.Console-3.15.2\bin\net6.0\nunit3-console.exe";
            string arguments = string.Format(@"C:\Users\lydin.camilleri\Desktop\Master's Code Repo\Appium Tests\AppiumTests\bin\Debug\net6.0\AppiumTests.dll --work=""C:\Users\lydin.camilleri\Desktop\Master's Code Repo\TestMate\TestMate.Runner\Logs\NUnit_TestResults\{0}"" --out=""Out.txt"" --result=""Result.xml"" > ""C:\Users\lydin.camilleri\Desktop\Master's Code Repo\TestMate\TestMate.Runner\Logs\NUnit_TestResults\{0}\ConsoleOutput.txt""", requestNumber);

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

            }

            // Run the tests
            //while (!cancellationToken.IsCancellationRequested)
            //{
            //    //TODO: Run the tests
            //    //TODO: Get Results

            //}
        }
    }
}
