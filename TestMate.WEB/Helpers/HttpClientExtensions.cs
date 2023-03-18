using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using TestMate.WEB.Controllers;

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

                if (response.StatusCode == HttpStatusCode.InternalServerError ||
                    response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new ApplicationException($"Something went wrong calling the API: {response.ReasonPhrase} \n {contentResult}");
                }
                //else if (response.StatusCode == HttpStatusCode.Unauthorized) {
                //    throw new UnauthorizedAccessException($"{response.ReasonPhrase}! \n {contentResult}");
                //}
            }

            try
            {
                var dataAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                return JsonSerializer.Deserialize<T>(dataAsString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception)
            {
                throw;
            }

        }

        public static string? GetSessionToken(this HttpContext httpContext)
        {
            return httpContext.Session.GetString("Token");
        }
    }
}
   
