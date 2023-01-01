using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TestMate.API.JWTAuthentication;
using TestMate.API.Settings;
using TestMate.Common.DataTransferObjects.Developers;
using TestMate.Common.Models.Developers;

namespace TestMate.API.Services;
public class DevelopersService
{
    private readonly IMongoCollection<Developer> _developersCollection;
    private readonly JWTAuthenticationService _jwtAuthenticationService;
    public DevelopersService(IOptions<DatabaseSettings> databaseSettings, JWTAuthenticationService jwtAuthenticationService)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _developersCollection = mongoDatabase.GetCollection<Developer>(databaseSettings.Value.DevelopersCollectionName);
        _jwtAuthenticationService = jwtAuthenticationService;
    }

    //Returns all developers
    public async Task<List<Developer>> GetAsync() => await _developersCollection.Find(_ => true).ToListAsync();

    //Returns a developer by username or null if not found
    public async Task<Developer?> GetAsync(string username) => await _developersCollection.Find(x => x.Username == username).FirstOrDefaultAsync();

    //Creates a new developer
    public async Task CreateAsync(Developer newDeveloper)
    {
        await _developersCollection.InsertOneAsync(newDeveloper);
    }

    //Updates developer (by username) 
    public async Task UpdateAsync(string username, Developer updatedDeveloper)
    {
        await _developersCollection.ReplaceOneAsync(x => x.Username == username, updatedDeveloper);
    }

    //Removes developer (by username)
    public async Task RemoveAsync(string username)
    {
        await _developersCollection.DeleteOneAsync(x => x.Username == username);
    }

    public async Task<DeveloperLoginResultDTO> Login(string username, string password)
    {
        var developer = await _developersCollection.Find(x => x.Username == username).FirstOrDefaultAsync();

        if (developer == null)
            return new DeveloperLoginResultDTO
            {
                Success = false,
                Message = $"Developer {username} does not exist!"
            };

        if (developer.Password != password)
            return new DeveloperLoginResultDTO
            {
                Success = false,
                Message = "Username/Password is not correct!"
            };

        return new DeveloperLoginResultDTO
        {
            Success = true,
            Token = _jwtAuthenticationService.GenerateJWTToken(username)
        };
    }
}
