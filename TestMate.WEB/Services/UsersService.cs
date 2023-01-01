
using TestMate.Common.Models.Users;
using TestMate.WEB.Helpers;
using TestMate.WEB.Services.Interfaces;

namespace TestMate.WEB.Services
{
    public class UsersService : IUsersService
    {
        private readonly HttpClient _client;
        private string BaseAddress;

        public UsersService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            BaseAddress = _client.BaseAddress.ToString();
        }

        public async Task<IEnumerable<User>> GetAllUserDetails()
        {
            var response = await _client.GetAsync(BaseAddress);

            return await response.ReadContentAsync<List<User>>();
        }

        public async Task<User> GetUserDetails(string username)
        {
            var response = await _client.GetAsync(BaseAddress + "/" + username);

            return await response.ReadContentAsync<User>();
        }
    }
}
