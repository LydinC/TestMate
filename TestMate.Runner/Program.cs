using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Driver;
using RabbitMQ.Client;
using TestMate.Common.Models.TestRequests;
using TestMate.Runner.BackgroundServices;

namespace TestMate.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
                {

                    // Settings for RabbitMQ Connection
                    var factory = new ConnectionFactory()
                    {
                        HostName = "localhost",
                        Port = 5672,
                        UserName = "guest",
                        Password = "guest"
                    };
                    var connection = factory.CreateConnection();
                    var channel = connection.CreateModel();



                    var mongoClient = new MongoClient("mongodb://localhost:27017");
                    var mongoDatabase = mongoClient.GetDatabase("testframework_db");


                    // Add the service
                    services.AddSingleton(connection);
                    services.AddSingleton(channel);
                    services.AddHostedService<RunnerService>();
                });
    }
}
