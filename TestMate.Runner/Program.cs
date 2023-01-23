using RabbitMQ.Client;
using Serilog;
using TestMate.Runner.BackgroundServices;


namespace TestMate.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An exception occurred while starting the host: {ex}");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog()
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

                    //var mongoClient = new MongoClient("mongodb://localhost:27017");
                    //var mongoDatabase = mongoClient.GetDatabase("testframework_db");

                    //Logging Configuration
                    var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .CreateLogger();

                    //TODO: CancellationTokenSource cts = new CancellationTokenSource();

                    // Add the services
                    services.AddSingleton(Log.Logger);
                    services.AddSingleton(connection);
                    services.AddSingleton(channel);
                    services.AddHostedService<RunnerService>();
                });
    }
}
