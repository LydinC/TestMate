using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RabbitMQ.Client;
using Serilog;
using System.Configuration;
using TestMate.Runner.BackgroundServices;
using TestMate.Runner.Settings;

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

                    var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

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
                    

                    services.Configure<DatabaseSettings>(configuration.GetSection("MongoDb"));

                    services.AddSingleton(sp =>
                    {
                        var options = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
                        var client = new MongoClient(options.ConnectionString);
                        return client.GetDatabase(options.DatabaseName);
                    });

                   
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .CreateLogger();

                    //TODO: CancellationTokenSource cts = new CancellationTokenSource();

                    // Add the services
                    services.AddSingleton(Log.Logger);
                    services.AddSingleton(connection);
                    services.AddSingleton(channel);
                    services.AddSingleton<DeviceManager>();
                    
                    services.AddHostedService<RunnerService>();
                    services.AddHostedService<MaintenanceService>();
                    services.AddHostedService<QueuingService>();
                });
    }
}
