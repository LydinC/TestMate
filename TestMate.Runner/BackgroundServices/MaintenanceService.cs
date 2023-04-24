using MongoDB.Driver;
using System.Threading;
using TestMate.Common.Utils;
using TestMate.Common.Models;
using TestMate.Common.Models.TestRequests;
using TestMate.Common.Models.Devices;
using TestMate.Common.Enums;

namespace TestMate.Runner.BackgroundServices
{ 
    public class MaintenanceService : BackgroundService
    {
        private readonly ILogger<MaintenanceService> _logger;
        private readonly IMongoCollection<Device> _devicesCollection;

        private readonly int _maintenanceInterval = 2; //minutes

        public MaintenanceService(IMongoDatabase database, ILogger<MaintenanceService> logger)
        {
            _logger = logger;
            _devicesCollection = database.GetCollection<Device>("Devices");
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try 
                {
                    await PerformMaintenanceRoutine(cancellationToken);
                }
                catch (Exception ex) 
                {
                    _logger.LogError(ex, "Error encountered during maintenance service routine.");
                }

                await Task.Delay(TimeSpan.FromMinutes(_maintenanceInterval), cancellationToken);
            }
        }

        private async Task PerformMaintenanceRoutine(CancellationToken cancellationToken)
        {

            await Task.Run(() =>
            {
                _logger.LogInformation("====== Maintenance Start ======");
                List<string> adbConnectedDevices = ConnectivityUtil.GetADBDevices();

                var filter = Builders<Device>.Filter.Where(d => d.Status == DeviceStatus.Connected && !adbConnectedDevices.Contains(d.IP));
                List<Device> devicesToCheck = _devicesCollection.Find(filter).ToList();

                int maintainedDevicesInDB = 0;
                int reconnectedDevicesInADB = 0;
                if (devicesToCheck.Count > 0)
                {
                    foreach(Device device in devicesToCheck)
                    {
                        if (!ConnectivityUtil.TryADBConnect(device.IP, device.TcpIpPort))
                        {
                            var update = Builders<Device>.Update.Set(d => d.Status, DeviceStatus.Offline);
                            _devicesCollection.UpdateOne(d => d.Id == device.Id, update);
                            maintainedDevicesInDB++;
                            _logger.LogInformation($"Failed to connect with device {device.IP} on port {device.TcpIpPort}. Switching to offline.");
                        }
                        else 
                        {
                            reconnectedDevicesInADB++;
                        }
                    }
                }
                
                _logger.LogInformation($"Maintained Devices in DB: {maintainedDevicesInDB}");
                _logger.LogInformation($"Reconnected Devices in ADB: {reconnectedDevicesInADB}");
                _logger.LogInformation("====== Maintenance End ======");

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }, cancellationToken);

        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Maintenance Service is stopping");
            await base.StopAsync(cancellationToken);
        }
    }
}
