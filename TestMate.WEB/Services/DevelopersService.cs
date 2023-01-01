using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using TestMate.Common.DataTransferObjects.Developers;
using TestMate.Common.Models.Developers;
using TestMate.WEB.Helpers;
using TestMate.WEB.Services.Interfaces;

namespace TestMate.WEB.Services
{
    public class DevelopersService : IDevelopersService
    {
        private readonly HttpClient _client;
        private string BaseAddress;

        public DevelopersService(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            BaseAddress = _client.BaseAddress.ToString();
        }

        public async Task<IEnumerable<Developer>> GetAllDeveloperDetails()
        {
            var response = await _client.GetAsync(BaseAddress);

            return await response.ReadContentAsync<List<Developer>>();
        }

        public async Task<Developer> GetDeveloperDetails(string username)
        {
            var response = await _client.GetAsync(BaseAddress + "/" + username);
            return await response.ReadContentAsync<Developer>();
        }
            
        public async Task<Developer> RegisterDeveloper(Developer newDeveloper)
        {
            var response = await _client.PostAsJsonAsync(BaseAddress + "/register", newDeveloper);

            return await response.ReadContentAsync<Developer>();

        }
        public async Task<DeveloperLoginResultDTO> Login(DeveloperLoginDTO developerLoginDTO) { 
       
            var response = await _client.PostAsJsonAsync(BaseAddress + "/login", developerLoginDTO);
            
            return await response.ReadContentAsync<DeveloperLoginResultDTO>();
            
        }

        public async Task<Developer> UpdateDeveloperDetails(string username, Developer updatedDeveloperDetails)
        {
            var response = await _client.PutAsJsonAsync(BaseAddress + "/update", updatedDeveloperDetails);

            return await response.ReadContentAsync<Developer>();
        }
    }
}
