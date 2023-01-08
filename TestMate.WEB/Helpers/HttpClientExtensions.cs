using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;
using TestMate.WEB.Services.Interfaces;

namespace TestMate.WEB.Helpers
{

    public static class HttpClientExtensions
    {
        public static async Task<T> ReadContentAsync<T>(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode == false)
            {
                Console.WriteLine($"Something went wrong calling the API: {response.ReasonPhrase} {JsonSerializer.Serialize(response.Content.ReadAsStringAsync().Result)}");
                var contentResult = response.Content.ReadAsStringAsync().Result;
                throw new ApplicationException($"Something went wrong calling the API: {response.ReasonPhrase} \n {contentResult}");
            }

            var dataAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var result = JsonSerializer.Deserialize<T>(
                dataAsString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return result;
        }

        public static string? GetSessionToken(this HttpContext httpContext)
        {
            return httpContext.Session.GetString("Token");
        }

    }
}
