using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TestMate.API.Settings;
using TestMate.Common.Models.Users;

namespace TestMate.API.Services
{
    public class UsersService
    {
        private readonly IMongoCollection<User> _usersCollection;

        public UsersService(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
            _usersCollection = mongoDatabase.GetCollection<User>(databaseSettings.Value.UsersCollectionName);
        }

        //Returns all users
        public async Task<List<User>> GetAsync() => await _usersCollection.Find(_ => true).ToListAsync();

        //Returns a user by username or null if not found
        public async Task<User?> GetAsync(string username) => await _usersCollection.Find(x => x.Username == username).FirstOrDefaultAsync();

        //Creates a new user
        public async Task CreateAsync(User newUser)
        {
            await _usersCollection.InsertOneAsync(newUser);
        }

        //TODO: See how we can update list of devices attachedto user
        //Update User
        public async Task UpdateAsync(string id, User user)
        {
            await _usersCollection.ReplaceOneAsync(x => x.Id == id, user);
        }

    }
}
