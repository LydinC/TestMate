using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace TestMate.Common.Utils
{
    public class RestApiUtil
    {
        private static string API_URL = "https://localhost:7112/api";
        private static HttpClient _client = new HttpClient();

        public static async Task<string> PostRequest(string url, string body = null, string token = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            return await CompleteRequest(request, body, token);
        }

        public static async Task<string> PostRequest(string url, object body, string token)
        {
            string bodyAsString = JsonConvert.SerializeObject(body);
            return await PostRequest(url, bodyAsString, token);
        }

        public static async Task<string> GetRequest(string url, string body = null, string token = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            return await CompleteRequest(request, body, token);
        }
        public static async Task<string> GetRequest(string url, object body, string token)
        {
            string bodyAsString = JsonConvert.SerializeObject(body);
            return await GetRequest(url, bodyAsString, token);
        }
        public static async Task<string> PutRequest(string url, string body = null, string token = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            return await CompleteRequest(request, body, token);
        }
        public static async Task<string> PutRequest(string url, object body, string token)
        {
            string bodyAsString = JsonConvert.SerializeObject(body);
            return await PutRequest(url, bodyAsString, token);
        }

        public static async Task<string> CompleteRequest(HttpRequestMessage request, string body, string token)
        {
            if (body != null)
            {
                request.Content = new StringContent(body);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        public static string BuildUrl(string path, string urlParams = "")
        {
            return String.Format("{0}/{1}{2}", API_URL, path, urlParams == null || urlParams == "" ? "" : "?" + urlParams);
        }
    }
}