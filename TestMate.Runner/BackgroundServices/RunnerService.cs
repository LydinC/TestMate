using TestMate.Common.Models.TestRequests;
using System;
using System.Text;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using RabbitMQ.Client;
using System.Threading.Tasks;
using AutoMapper.Internal;
using Newtonsoft.Json;
using ZstdSharp.Unsafe;
using System.Threading.Channels;
using RabbitMQ.Client.Events;
using System.Net.Sockets;
using System.Net;
using OpenQA.Selenium.Appium.Android;
using MongoDB.Driver.Core.Bindings;
using System.Threading;

namespace TestMate.Runner.BackgroundServices
{
    class RunnerService : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RunnerService(IConnection connection, IModel channel) {
            _connection = connection;
            _channel = channel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            //Exchange Declaration
            string exchangeName = "TestRequestExchange";
            _channel.ExchangeDeclare(
                exchange: exchangeName, 
                type: ExchangeType.Direct);

            // Declare a queue for test request messages.
            string queueName = "test-requests";
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            //Bind queue to exchange
            _channel.QueueBind(
                queue: queueName, 
                exchange: exchangeName, 
                routingKey: "testRequestKey",
                arguments: null);


            /* CONSUMER */
            // Start consuming messages from the queue
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                // Get the message body
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                // Process the message
                Console.WriteLine("Message received: " + message);
                int availablePort = FindAvailablePort();

                // Run the Appium tests in a separate thread
                var thread = new Thread(() => RunAppiumTests(stoppingToken, availablePort));
                thread.Start();

                // Acknowledge the message
                _channel.BasicAck(
                    deliveryTag: ea.DeliveryTag, 
                    multiple: false);
            };

            _channel.BasicConsume(
                queue: queueName, 
                autoAck: false, 
                consumer: consumer);


            /* LISTENER FOR PUBLISHER */

            // Connect to MongoDB
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            var database = mongoClient.GetDatabase("testframework_db");
            var collection = database.GetCollection<TestRequest>("TestRequests");

            // Setting up a ChangeStream with the TestRequests MongoDB collection
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<TestRequest>>();
            var options = new ChangeStreamOptions();
            options.FullDocument = ChangeStreamFullDocumentOption.UpdateLookup;

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
                        Console.WriteLine("Publishing message: {0}", message);

                        PublishMessage(message);
                    }
                });
            }


            ////CONSUMER

            //// Create a consumer to handle messages from the queue
            //var consumer = new EventingBasicConsumer(_channel);
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

            // Wait for the service to stop
            await Task.Delay(-1, stoppingToken);
        }

        static void PublishMessage(string message)
        {
            string exchangeName = "TestRequestExchange";

            // Connect to RabbitMQ
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Declare a queue for test request messages.
                channel.QueueDeclare(
                    queue: "test-requests",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                
                // Publish the message to the queue
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(
                    exchange: exchangeName,
                    routingKey: "testRequestKey",
                    basicProperties: null,
                    body: body
                    );
            }
        }

        static string SerializeTestRequest(TestRequest testRequest)
        {
            // Serialize the test request object to a string
            return JsonConvert.SerializeObject(testRequest);
        }

        static TestRequest? DeserializeTestRequest(string message)
        {
            // Serialize the test request object to a string
            return JsonConvert.DeserializeObject<TestRequest>(message);
        }


        static int FindAvailablePort()
        {
            // Find an available port by trying to bind a socket to a series of ports and seeing which ones are available
            // This is just one way to find an available port - you could also use other methods
            var port = 4723; // Appium default port
            while (true)
            {
                try
                {
                    using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
                        return port;
                    }
                }
                catch (SocketException)
                {
                    port++;
                }
            }
        }


        private void RunAppiumTests(CancellationToken stoppingToken, int port) {
            
            // Set up the Appium connection and run the tests on the specified port
            Console.WriteLine("Running Appium tests on port " + port);

            // Run the tests
            while (!stoppingToken.IsCancellationRequested)
            {
                //TODO: Run the tests
                //TODO: Get Results
                
            }
        }
    }
}
