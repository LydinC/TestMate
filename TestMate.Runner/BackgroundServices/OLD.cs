//using TestMate.Common.Models.TestRequests;
//using System;
//using System.Text;
//using Microsoft.Extensions.Hosting;
//using MongoDB.Driver;
//using RabbitMQ.Client;
//using System.Threading.Tasks;
//using AutoMapper.Internal;
//using Newtonsoft.Json;

//namespace TestMate.Runner.BackgroundServices
//{
//    class OLD : BackgroundService
//    {
//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            // Connect to MongoDB
            

            
//            // Setting up a ChangeStream with the TestRequests MongoDB collection
//            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<TestRequest>>();
//            var options = new ChangeStreamOptions();
//            options.FullDocument = ChangeStreamFullDocumentOption.UpdateLookup;


//            using (var changeStream = collection.Watch(pipeline, options))
//            {
//                // Listen for change events
//                await changeStream.ForEachAsync(async change =>
//                {
//                    // Check if the change is a new insert
//                    if (change.OperationType == ChangeStreamOperationType.Insert)
//                    {
//                        // Publish the test request as a message to RabbitMQ Queue
//                        var testRequest = change.FullDocument;
//                        var message = SerializeTestRequest(testRequest);
//                        Console.WriteLine("Publishing message: {0}", message);

//                        PublishMessage(message);
//                    }
//                });
//            }


//        }

//        static void PublishMessage(string message)
//        {

//            string queueName = "test-requests";

//            // Connect to RabbitMQ
//            var factory = new ConnectionFactory() { HostName = "localhost" };
//            using (var connection = factory.CreateConnection())
//            using (var channel = connection.CreateModel())
//            {
//                // Declare a queue for test request messages.
//                channel.QueueDeclare(
//                    queue: "test-requests",
//                    durable: true,
//                    exclusive: false,
//                    autoDelete: false,
//                    arguments: null);
                
//                // Publish the message to the queue
//                var body = Encoding.UTF8.GetBytes(message);
//                channel.BasicPublish(
//                    exchange: "",
//                    routingKey: queueName,
//                    basicProperties: null,
//                    body: body
//                    );
//            }
//        }

//        static string SerializeTestRequest(TestRequest testRequest)
//        {
//            // Serialize the test request object to a string
//            return JsonConvert.SerializeObject(testRequest);
//        }
//    }
//}
