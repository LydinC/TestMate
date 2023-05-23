using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using TestMate.API.Settings;
using TestMate.Common.DataTransferObjects.APIResponse;
using TestMate.Common.DataTransferObjects.Developers;
using TestMate.Common.DataTransferObjects.Devices;
using TestMate.Common.Enums;
using TestMate.Common.Models.Developers;
using TestMate.Common.Models.Devices;
using TestMate.Common.Utils;

namespace TestMate.API.Services;
public class DevicesService
{
    private readonly IMongoCollection<Device> _devicesCollection;
    private readonly IMapper _mapper;
    private readonly ILogger<DevicesService> _logger;

    public DevicesService(IOptions<DatabaseSettings> databaseSettings, IMapper mapper, ILogger<DevicesService> logger)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _devicesCollection = mongoDatabase.GetCollection<Device>(databaseSettings.Value.DevicesCollectionName);
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<APIResponse<IEnumerable<Device>>> GetAllDevices()
    {
        
        try
        {
            _logger.LogInformation($"[{Task.CurrentId}] - Called GetAllDevices API");
            var devices = await _devicesCollection.Find(_ => true).ToListAsync();
            _logger.LogInformation($"[{Task.CurrentId}] - {devices.Count} devices retrieved");
            return new APIResponse<IEnumerable<Device>>(devices);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[{Task.CurrentId}] - Exception encountered. {ex.Message} {ex.StackTrace}.");
            return new APIResponse<IEnumerable<Device>>(Status.Error, ex.Message);
        }
    }
    public async Task<APIResponse<DeviceStatus>> GetStatusByIP(string ip)
    {
        try
        {
            _logger.LogInformation($"[{Task.CurrentId}] - Called GetStatusByIP API - {ip}");
            var device = await _devicesCollection.Find(d => d.IP == ip).FirstOrDefaultAsync();

            if (device == null)
            {
                _logger.LogError($"[{Task.CurrentId}] - Device with IP {ip} is not yet registered!");
                return new APIResponse<DeviceStatus>(Status.Error, $"Device with IP {ip} is not yet registered!");
            }

            _logger.LogInformation($"[{Task.CurrentId}] - Device Status retrieved as {device.Status}");
            return new APIResponse<DeviceStatus>(device.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[{Task.CurrentId}] - Exception encountered. {ex.Message} {ex.StackTrace}.");
            return new APIResponse<DeviceStatus>(Status.Error, ex.Message);
        }
    }


    public async Task<APIResponse<Device>> GetDeviceBySerialNumber(string serialNumber)
    {
        try
        {
            _logger.LogInformation($"[{Task.CurrentId}] - Called GetDeviceBySerialNumber API - {serialNumber}");
            
            var device = await _devicesCollection.Find(x => x.SerialNumber == serialNumber).FirstOrDefaultAsync();

            if (device == null)
            {
                _logger.LogError($"[{Task.CurrentId}] - Device details could not be retrieved!");
                return new APIResponse<Device>(Status.Error, "Device details could not be retrieved!");
            }

            _logger.LogInformation($"[{Task.CurrentId}] - Device retrieved: {device.ToJson()}");
            return new APIResponse<Device>(device);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[{Task.CurrentId}] - Exception encountered. {ex.Message} {ex.StackTrace}.");
            return new APIResponse<Device>(Status.Error, ex.Message);
        }
    }

    public async Task<APIResponse<bool>> DisconnectDevice(DevicesConnectDTO deviceDTO)
    {
        try
        {
            _logger.LogInformation($"[{Task.CurrentId}] - Called DisconnectDevice API - {deviceDTO.ToJson()}");
            var device = await _devicesCollection.Find(x => x.IP == deviceDTO.IP).FirstOrDefaultAsync();

            if (device == null)
            {
                _logger.LogError($"[{Task.CurrentId}] - Device could not be retrieved!");
                return new APIResponse<bool>(Status.Error, "Device could not be retrieved!", false);
            }

            _logger.LogInformation($"[{Task.CurrentId}] - Executing disconnect adb device.");
            ConnectivityUtil.DisconnectADBDevice(device.IP, device.TcpIpPort);

            _logger.LogInformation($"[{Task.CurrentId}] - Updating device status to OFFLINE in DB!");
            var filter = Builders<Device>.Filter.Eq(x => x.SerialNumber, device.SerialNumber);
            var update = Builders<Device>.Update
                .Set(d => d.Status, DeviceStatus.Offline);

            await _devicesCollection.UpdateOneAsync(filter, update);
         
            return new APIResponse<bool>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[{Task.CurrentId}] - Exception encountered. {ex.Message} {ex.StackTrace}.");
            return new APIResponse<bool>(Status.Error, ex.Message, false);
        }
    }


    public async Task<APIResponse<Device>> ConnectDevice(DevicesConnectDTO deviceDTO)
    {

        _logger.LogInformation($"[{Task.CurrentId}] - ConnectDevice API - " + deviceDTO.ToJson());

        //Validate IP Address
        if (!IPAddress.TryParse(deviceDTO.IP, out var address))
        {
            _logger.LogError($"[{Task.CurrentId}] - IP {deviceDTO.IP} is invalid!");
            return new APIResponse<Device>(Status.Error, $"IP {deviceDTO.IP} is invalid!");
        }

        //Validate IP is reachable through PING
        if (!ConnectivityUtil.IsIpReachable(ip: deviceDTO.IP, timeoutInMs: 10000))
        {
            _logger.LogError($"[{Task.CurrentId}] - IP {deviceDTO.IP} is not reachable. Cannot connect!");
            return new APIResponse<Device>(Status.Error, $"IP {deviceDTO.IP} is not reachable. Cannot connect!");
        }

        int port;
        try
        {
            List<string> adbDevices = ConnectivityUtil.GetADBDevices();

            if (adbDevices == null || adbDevices.Count == 0)
            {
                _logger.LogError($"[{Task.CurrentId}] - ADB does not indicate any connected devices!");
                return new APIResponse<Device>(Status.Error, "ADB does not indicate any connected devices!");
            }
            else
            {
                //Check if Device with same IP Exists in Database
                Device deviceInDbWithSameIP = await _devicesCollection.Find(x => x.IP == deviceDTO.IP).FirstOrDefaultAsync();

                //Is the device already ADB connected?
                if (adbDevices.Any(x => x.Contains(deviceDTO.IP + ":")))
                {
                    _logger.LogInformation($"[{Task.CurrentId}] - Device with IP {deviceDTO.IP} is already connected via ADB. Validating properties ...");

                    //Get the serial number for the IP trying to connect and which is already connected in ADB
                    string serial = ConnectivityUtil.GetSerialNumberByIp(deviceDTO.IP);
                    if (!string.IsNullOrEmpty(serial))
                    {
                        if (deviceInDbWithSameIP != null)
                        {
                            if (deviceInDbWithSameIP.SerialNumber != serial)
                            {
                                //TODO: Should i delete the MongoDB Document and update it with the new details?
                            }
                            else
                            {
                                //TODO: Make sure the Status is connected in MongoDB for the deviceInDbWithSameIP
                                //Update Properties too?
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError($"[{Task.CurrentId}] - Could not retrieve serial number for device with IP {deviceDTO.IP}");
                        return new APIResponse<Device>(Status.Error, $"Could not retrieve serial number for device with IP {deviceDTO.IP}");
                    }

                    //TODO: Make sure that device in DB is correct (same Ip + same Serial + Status is connected?)
                    _logger.LogInformation($"[{Task.CurrentId}] - Device with IP {deviceDTO.IP} is already connected!");
                    return new APIResponse<Device>(Status.Ok, $"Device with IP {deviceDTO.IP} is already connected!");
                }
                else
                {
                    _logger.LogInformation($"[{Task.CurrentId}] - Device with IP {deviceDTO.IP} is not currently connected via ADB!");

                    port = ConnectivityUtil.GetAvailableTcpIpPort();
                    _logger.LogInformation($"[{Task.CurrentId}] - Available TCP/IP port identified as {port}");

                    string? serialNumber = adbDevices.FirstOrDefault(x => !Regex.IsMatch(x, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"));
                    if (serialNumber == null)
                    {
                        _logger.LogError($"[{Task.CurrentId}] - Could not retrieve ADB Serial Number for device with IP {deviceDTO.IP}. Make sure you connect using USB the first time.");
                        return new APIResponse<Device>(Status.Error, $"Could not retrieve ADB Serial Number for device with IP {deviceDTO.IP}. Make sure you connect using USB the first time.");
                    }

                    if (!ConnectivityUtil.ValidateSerialNumberAndIP(serialNumber, deviceDTO.IP))
                    {
                        _logger.LogError($"[{Task.CurrentId}] - The Serial Number {serialNumber} does not own requested IP {deviceDTO.IP}.");
                        return new APIResponse<Device>(Status.Error, $"The Serial Number {serialNumber} does not own requested IP {deviceDTO.IP}.");
                    }

                    if (!ConnectivityUtil.TryADBConnect(serialNumber, deviceDTO.IP, port))
                    {
                        _logger.LogError($"[{Task.CurrentId}] - Could not connect to ADB for IP {deviceDTO.IP}, Port: {port}.");
                        return new APIResponse<Device>(Status.Error, $"Could not connect to ADB for IP {deviceDTO.IP}, Port: {port}.");
                    }

                    _logger.LogInformation($"[{Task.CurrentId}] - Connected device with IP {deviceDTO.IP} successfully to ADB");

                    try
                    {
                        _logger.LogInformation($"[{Task.CurrentId}] - Getting device properties");
                        DeviceProperties properties = ConnectivityUtil.GetDeviceProperties(deviceDTO.IP, port);
                        _logger.LogInformation($"[{Task.CurrentId}] - Device properties retrieved as {properties.ToJson()}");

                        // Insert or update device record
                        var filter = Builders<Device>.Filter.Eq(x => x.SerialNumber, serialNumber);
                        var update = Builders<Device>.Update
                            .Set(d => d.SerialNumber, serialNumber)
                            .Set(d => d.DeviceProperties, properties)
                            .Set(d => d.TcpIpPort, port)
                            .Set(d => d.Status, DeviceStatus.Connected)
                            .Set(d => d.ConnectedTimestamp, DateTime.UtcNow)
                            .Set(d => d.IP, deviceDTO.IP);

                        await _devicesCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
                        _logger.LogInformation($"[{Task.CurrentId}] - Upserted device with IP {deviceDTO.IP} in devices collection.");
                        
                        return new APIResponse<Device>(Status.Ok, "Device Successfully Connected!");

                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"[{Task.CurrentId}] - Exception encountered. {e.Message} {e.StackTrace}.");
                        ConnectivityUtil.DisconnectADBDevice(deviceDTO.IP, port);
                        _logger.LogError($"[{Task.CurrentId}] - Executed disconnect ADB device command");
                        throw;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"[{Task.CurrentId}] - Exception encountered. {ex.Message} {ex.StackTrace}.");
            return new APIResponse<Device>(Status.Error, ex.Message);
        }
    }

    
    public async Task<APIResponse<Device>> UpdateAsync(Device updatedDevice)
    {

        try
        {
            _logger.LogInformation($"[{Task.CurrentId}] - UpdateDevice API - " + updatedDevice.ToJson());

            var device = await _devicesCollection.Find(x => x.SerialNumber == updatedDevice.SerialNumber).FirstOrDefaultAsync();
            if (device == null) return new APIResponse<Device>(Status.Error, "Device could not be updated!");

            var result = await _devicesCollection.ReplaceOneAsync(x => x.SerialNumber == updatedDevice.SerialNumber, updatedDevice);

            _logger.LogInformation($"[{Task.CurrentId}] - Successfully updated device with SN {updatedDevice.SerialNumber}");
            return new APIResponse<Device>(updatedDevice);
        }
        catch (Exception ex)
        {
            _logger.LogError($"[{Task.CurrentId}] - Exception encountered. {ex.Message} {ex.StackTrace}.");
            return new APIResponse<Device>(Status.Error, ex.Message);
        }
    }

}