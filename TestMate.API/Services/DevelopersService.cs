using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TestMate.API.Services.Interfaces;
using TestMate.API.Settings;
using TestMate.Common.DataTransferObjects.APIResponse;
using TestMate.Common.DataTransferObjects.Developers;
using TestMate.Common.Enums;
using TestMate.Common.Models.Developers;

namespace TestMate.API.Services;
public class DevelopersService : IDevelopersService
{
    private readonly IMongoCollection<Developer> _developersCollection;
    private readonly JWTAuthenticationService _jwtAuthenticationService;
    private readonly IMapper _mapper;

    public DevelopersService(IOptions<DatabaseSettings> databaseSettings, IMapper mapper, JWTAuthenticationService jwtAuthenticationService)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _developersCollection = mongoDatabase.GetCollection<Developer>(databaseSettings.Value.DevelopersCollectionName);
        _mapper = mapper;
        _jwtAuthenticationService = jwtAuthenticationService;
    }
    public async Task<APIResponse<DeveloperLoginResultDTO>> Login(DeveloperLoginDTO developerLoginDTO)
    {
        try
        {
            var developer = await _developersCollection.Find(x => x.Username == developerLoginDTO.Username).FirstOrDefaultAsync();

            if (developer == null || developer.Password != developerLoginDTO.Password)
                return new APIResponse<DeveloperLoginResultDTO>(Status.Error, "Failed to log in. Please check your username and password and try again.");

            return new APIResponse<DeveloperLoginResultDTO>(Status.Ok, "Successfully logged in!", new DeveloperLoginResultDTO { Token = _jwtAuthenticationService.GenerateJWTToken(developerLoginDTO.Username) });

        }
        catch (Exception ex)
        {
            return new APIResponse<DeveloperLoginResultDTO>(Status.Error, ex.Message);
        }
    }

    public async Task<APIResponse<IEnumerable<Developer>>> GetAllDevelopers()
    {
        try
        {
            var developers = await _developersCollection.Find(_ => true).ToListAsync();
            return new APIResponse<IEnumerable<Developer>>(developers);
        }
        catch (Exception ex)
        {
            return new APIResponse<IEnumerable<Developer>>(Status.Error, ex.Message);
        }
    }


    public async Task<APIResponse<Developer>> GetDeveloper(string username)
    {
        try
        {
            var developer = await _developersCollection.Find(x => x.Username == username).FirstOrDefaultAsync();

            if (developer == null) return new APIResponse<Developer>(Status.Error, "Developer details could not be retrieved!");

            return new APIResponse<Developer>(developer);
        }
        catch (Exception ex)
        {
            return new APIResponse<Developer>(Status.Error, ex.Message);
        }
    }


    public async Task<APIResponse<DeveloperRegisterResultDTO>> Register(DeveloperRegisterDTO developerRegisterDTO)
    {
        try
        {
            Developer developer = _mapper.Map<Developer>(developerRegisterDTO);
            var developers = await _developersCollection.Find(_ => true).ToListAsync();

            if (developers != null)
            {
                if (developers.Any(i => i.Username == developer.Username || i.Email == developer.Email))
                {
                    return new APIResponse<DeveloperRegisterResultDTO>(Status.Error, "Username/Email already registered!");
                }
            }

            developer.IsActive = true;
            await _developersCollection.InsertOneAsync(developer);
            return new APIResponse<DeveloperRegisterResultDTO>(Status.Ok, "Developer Account Successfully Created", new DeveloperRegisterResultDTO(developer.Username));
        }
        catch (Exception ex)
        {
            return new APIResponse<DeveloperRegisterResultDTO>(Status.Error, ex.Message);
        }
    }

    // STILL TO BE REVISED

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

}