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

    public DevicesService(IOptions<DatabaseSettings> databaseSettings, IMapper mapper)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _devicesCollection = mongoDatabase.GetCollection<Device>(databaseSettings.Value.DevicesCollectionName);
        _mapper = mapper;
    }


    public async Task<APIResponse<IEnumerable<Device>>> GetAllDevices()
    {
        try
        {
            var devices = await _devicesCollection.Find(_ => true).ToListAsync();
            return new APIResponse<IEnumerable<Device>>(devices);
        }
        catch (Exception ex)
        {
            return new APIResponse<IEnumerable<Device>>(Status.Error, ex.Message);
        }
    }
    public async Task<APIResponse<DeviceStatus>> GetStatusByIP(string ip)
    {
        try
        {
            var device = await _devicesCollection.Find(d => d.IP == ip).FirstOrDefaultAsync();
            
            if (device == null) 
            {
                return new APIResponse<DeviceStatus>(Status.Error, "Unable to get device with IP: " + ip);
            }

            return new APIResponse<DeviceStatus>(device.Status);
        }
        catch (Exception ex)
        {
            return new APIResponse<DeviceStatus>(Status.Error, ex.Message);
        }
    }


    
    public async Task<APIResponse<Device>> GetDeviceBySerialNumber(string SerialNumber)
    {
        try
        {
            var device = await _devicesCollection.Find(x => x.SerialNumber == SerialNumber).FirstOrDefaultAsync();

            if (device == null) return new APIResponse<Device>(Status.Error, "Device details could not be retrieved!");

            return new APIResponse<Device>(device);
        }
        catch (Exception ex)
        {
            return new APIResponse<Device>(Status.Error, ex.Message);
        }
    }

    public async Task<APIResponse<Device>> ConnectDevice(DevicesConnectDTO deviceDTO)
    {

        //Validate IP Address
        if (!IPAddress.TryParse(deviceDTO.IP, out var address))
        {
            return new APIResponse<Device>(Status.Error, "IP " + deviceDTO.IP + " is invalid!");
        }

        //Validate IP is reachable through PING
        if (!ConnectivityUtil.IsIpReachable(ip: deviceDTO.IP, timeoutInMs: 10000))
        {
            return new APIResponse<Device>(Status.Error, "IP " + deviceDTO.IP + " is not reachable. Cannot connect!");
        }

        int port;
        try
        {
            List<string> adbDevices = ConnectivityUtil.GetADBDevices();
            if(adbDevices == null || adbDevices.Count == 0)
            {
                return new APIResponse<Device>(Status.Error, "ADB does not indicate any connected devices!");
            } 
            else
            {
                //Check if Device with same IP Exists in Database
                Device deviceInDbWithSameIP = await _devicesCollection.Find(x => x.IP == deviceDTO.IP).FirstOrDefaultAsync();

                //Is the device already ADB connected?
                if (adbDevices.Any(x => x.Contains(deviceDTO.IP)))
                {
                    //Get the serial number for the IP trying to connect and which is already connected in ADB
                    string serial = ConnectivityUtil.GetSerialNumberByIp(deviceDTO.IP);
                    if (!string.IsNullOrEmpty(serial))
                    {
                        if (deviceInDbWithSameIP != null)
                        {
                            if(deviceInDbWithSameIP.SerialNumber != serial)
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
                        return new APIResponse<Device>(Status.Error, "Could not retrieve serial number for device with IP: " + deviceDTO.IP);
                    }

                    //TODO: Make sure that device in DB is correct (same Ip + same Serial + Status is connected?)
                    return new APIResponse<Device>(Status.Ok, "Device with IP " + deviceDTO.IP + " is already connected!");
                } 
                else
                {
                    port = ConnectivityUtil.GetAvailableTcpIpPort();

                    string? serialNumber = adbDevices.FirstOrDefault(x => !Regex.IsMatch(x, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"));
                    if (serialNumber == null)
                    {
                        return new APIResponse<Device>(Status.Error, "Could not retrieve ADB Serial Number for device with IP " + deviceDTO.IP + ". Make sure you connect using USB the first time.");
                    }

                    if (!ConnectivityUtil.ValidateSerialNumberAndIP(serialNumber, deviceDTO.IP)) {
                        return new APIResponse<Device>(Status.Error, "The Serial Number " + serialNumber + " does not own requested IP " + deviceDTO.IP + ".");
                    }

                    if (!ConnectivityUtil.TryADBConnect(serialNumber, deviceDTO.IP, port))
                    {
                        return new APIResponse<Device>(Status.Error, "Could not connect to ADB for IP: " + deviceDTO.IP + ", Port: " + port + ".");
                    }

                    try
                    {
                        DeviceProperties properties = ConnectivityUtil.GetDeviceProperties(deviceDTO.IP, port);

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

                        return new APIResponse<Device>(Status.Ok, "Device Successfully Connected!");

                    }
                    catch (Exception)
                    {
                        ConnectivityUtil.DisconnectADBDevice(deviceDTO.IP, port);
                        throw;
                    }
                }
            }


            
            
        }
        catch (Exception ex)
        {
            
            return new APIResponse<Device>(Status.Error, ex.Message);
        }
    }

    public async Task<APIResponse<Device>> UpdateAsync(Device updatedDevice)
    {
        try
        {
            var device = await _devicesCollection.Find(x => x.SerialNumber == updatedDevice.SerialNumber).FirstOrDefaultAsync();
            if (device == null) return new APIResponse<Device>(Status.Error, "Device could not be updated!");

            var result = await _devicesCollection.ReplaceOneAsync(x => x.SerialNumber == updatedDevice.SerialNumber, updatedDevice);

            return new APIResponse<Device>(updatedDevice);
        }
        catch (Exception ex)
        {
            return new APIResponse<Device>(Status.Error, ex.Message);
        }   
    }

    //Removes devices (by SerialNumber)
    public async Task RemoveAsync(string SerialNumber)
    {
        await _devicesCollection.DeleteOneAsync(x => x.SerialNumber == SerialNumber);
    }
}
