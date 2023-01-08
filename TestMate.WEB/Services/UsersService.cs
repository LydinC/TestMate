
using TestMate.Common.Models.Users;
using TestMate.WEB.Helpers;
using TestMate.WEB.Services.Interfaces;

namespace TestMate.WEB.Services
{
    public class UsersService : IUsersService
    {
        private readonly HttpClient _client;
        private readonly ILogger<UsersService> _logger;
        private readonly string _baseAddress = new Uri("https://localhost:7112/api/users").ToString();

        public UsersService(HttpClient client, ILogger<UsersService> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAllUserDetails()
        {
            var response = await _client.GetAsync(_baseAddress);

            return await response.ReadContentAsync<List<User>>();
        }

        public async Task<User> GetUserDetails(string username)
        {
            var response = await _client.GetAsync(_baseAddress + "/" + username);

            return await response.ReadContentAsync<User>();
        }
    }
}
