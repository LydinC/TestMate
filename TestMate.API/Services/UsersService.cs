using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TestMate.API.Settings;
using TestMate.Common.DataTransferObjects.APIResponse;
using TestMate.Common.DataTransferObjects.Developers;
using TestMate.Common.DataTransferObjects.Users;
using TestMate.Common.Enums;
using TestMate.Common.Models.Developers;
using TestMate.Common.Models.Users;

namespace TestMate.API.Services
{
    public class UsersService
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly JWTAuthenticationService _jwtAuthenticationService;
        private readonly IMapper _mapper;

        public UsersService(IMapper mapper, JWTAuthenticationService jwtAuthenticationService, IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
            _mapper = mapper; 
            _usersCollection = mongoDatabase.GetCollection<User>(databaseSettings.Value.UsersCollectionName);
            _jwtAuthenticationService = jwtAuthenticationService;
        }

        public async Task<APIResponse<IEnumerable<User>>> GetAllUsers()
        {
            try
            {
                var users = await _usersCollection.Find(_ => true).ToListAsync();
                return new APIResponse<IEnumerable<User>>(users);
            }
            catch (Exception ex)
            {
                return new APIResponse<IEnumerable<User>>(Status.Error, ex.Message);
            }
        }

        public async Task<APIResponse<User>> GetUser(string username)
        {
            try
            {
                var user = await _usersCollection.Find(x => x.Username == username).FirstOrDefaultAsync();

                if (user == null) return new APIResponse<User>(Status.Error, "User details could not be retrieved!");

                return new APIResponse<User>(user);
            }
            catch (Exception ex)
            {
                return new APIResponse<User>(Status.Error, ex.Message);
            }
        }



        public async Task<APIResponse<UserLoginResultDTO>> Login(UserLoginDTO userLoginDTO)
        {
            try
            {
                var user = await _usersCollection.Find(x => x.Username == userLoginDTO.Username).FirstOrDefaultAsync();

                if (user == null || user.Password != userLoginDTO.Password)
                    return new APIResponse<UserLoginResultDTO>(Status.Error, "Failed to log in. Please check your username and password and try again.");

                return new APIResponse<UserLoginResultDTO>(Status.Ok, "Successfully logged in!", new UserLoginResultDTO { Token = _jwtAuthenticationService.GenerateJWTToken(userLoginDTO.Username) });
            }
            catch (Exception ex)
            {
                return new APIResponse<UserLoginResultDTO>(Status.Error, ex.Message);
            }
        }

        public async Task<APIResponse<UserRegisterResultDTO>> Register(UserRegisterDTO userRegisterDTO)
        {
            try
            {
                User user = _mapper.Map<User>(userRegisterDTO);
                var users = await _usersCollection.Find(_ => true).ToListAsync();

                if (users != null)
                {
                    if (users.Any(i => i.Username == user.Username || i.Email == user.Email))
                    {
                        return new APIResponse<UserRegisterResultDTO>(Status.Error, "Username/Email already registered!");
                    }
                }

                user.IsActive = true;
                await _usersCollection.InsertOneAsync(user);
                return new APIResponse<UserRegisterResultDTO>(Status.Ok, "User Account Successfully Created", new UserRegisterResultDTO(user.Username));
            }
            catch (Exception ex)
            {
                return new APIResponse<UserRegisterResultDTO>(Status.Error, ex.Message);
            }
        }

        //TODO: See how we can update list of devices attachedto user
        //Update User
        public async Task UpdateAsync(string id, User user)
        {
            await _usersCollection.ReplaceOneAsync(x => x.Id == id, user);
        }

    }
}
