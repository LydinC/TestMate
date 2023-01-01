using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TestMate.API.Settings;
using TestMate.Common.Models.Devices;

namespace TestMate.API.Services;
public class DevicesService
{
    private readonly IMongoCollection<Device> _devicesCollection;

    public DevicesService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _devicesCollection = mongoDatabase.GetCollection<Device>(databaseSettings.Value.DevicesCollectionName);
    }

    //Returns all devices
    public async Task<List<Device>> GetAsync() => await _devicesCollection.Find(_ => true).ToListAsync();

    //Returns a device by IMEI or null if not found
    public async Task<Device?> GetAsync(string IMEI) => await _devicesCollection.Find(x => x.IMEI == IMEI).FirstOrDefaultAsync();

    //Creates a new device
    public async Task CreateAsync(Device newDevice)
    {
        await _devicesCollection.InsertOneAsync(newDevice);
    }

    //Updates device (by IMEI) 
    public async Task UpdateAsync(string IMEI, Device updatedDevice)
    {
        await _devicesCollection.ReplaceOneAsync(x => x.IMEI == IMEI, updatedDevice);
    }

    //Removes devices (by IMEI)
    public async Task RemoveAsync(string IMEI)
    {
        await _devicesCollection.DeleteOneAsync(x => x.IMEI == IMEI);
    }
}
