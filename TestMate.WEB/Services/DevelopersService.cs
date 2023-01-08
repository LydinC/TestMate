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
        private readonly ILogger<DevelopersService> _logger;
        private readonly string _baseAddress = new Uri("https://localhost:7112/api/developers").ToString();

        public DevelopersService(HttpClient client, ILogger<DevelopersService> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger;
        }

        public async Task<IEnumerable<Developer>> GetAllDeveloperDetails()
        {
            var response = await _client.GetAsync(_baseAddress);

            return await response.ReadContentAsync<List<Developer>>();
        }

        public async Task<Developer> GetDeveloperDetails(string username)
        {
            var response = await _client.GetAsync(_baseAddress + "/" + username);
            return await response.ReadContentAsync<Developer>();
        }
            
        public async Task<Developer> RegisterDeveloper(Developer newDeveloper)
        {
            var response = await _client.PostAsJsonAsync(_baseAddress + "/register", newDeveloper);

            return await response.ReadContentAsync<Developer>();

        }
        public async Task<DeveloperLoginResultDTO> Login(DeveloperLoginDTO developerLoginDTO) { 
       
            var response = await _client.PostAsJsonAsync(_baseAddress + "/login", developerLoginDTO);
            
            return await response.ReadContentAsync<DeveloperLoginResultDTO>();
            
        }

        public async Task<Developer> UpdateDeveloperDetails(string username, Developer updatedDeveloperDetails)
        {
            var response = await _client.PutAsJsonAsync(_baseAddress + "/update", updatedDeveloperDetails);

            return await response.ReadContentAsync<Developer>();
        }
    }
}
