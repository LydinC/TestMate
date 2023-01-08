
using System.Net.Http.Json;
using TestMate.Common.DataTransferObjects.TestRequests;
using TestMate.Common.Models.TestRequests;
using TestMate.WEB.Helpers;
using TestMate.WEB.Services.Interfaces;

namespace TestMate.WEB.Services
{
    public class TestRequestsService : ITestRequestsService
    {
        private readonly HttpClient _client;
        private readonly ILogger<TestRequestsService> _logger;
        private readonly string _baseAddress = new Uri("https://localhost:7112/api/testrequests").ToString();

        public TestRequestsService(HttpClient client, ILogger<TestRequestsService> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger;
        }

        public async Task<IEnumerable<TestRequest>> GetAllTestRequests()
        {
            var response = await _client.GetAsync(_baseAddress);

            return await response.ReadContentAsync<List<TestRequest>>();
        }

        public async Task<TestRequest> GetTestRequestDetails(string username)
        {
            var response = await _client.GetAsync(_baseAddress + "/" + username);

            return await response.ReadContentAsync<TestRequest>();
        }


        public async Task<TestRequestWebCreateResult> CreateTestRequest(TestRequestWebCreateDTO newTestRequest)
        {

            //UPLOAD IFORMFILES TO CENTRAL LOCATION
            //TODO: COnsider using _webHostEnvironment instead of hardcoded location
            var testRequestID = GetUniqueTestRequestID();
            var testRequestFolder = Path.Combine("C:/Users/lydin.camilleri/Desktop/Master's Code Repo", "Uploads", testRequestID);

            if (newTestRequest.AppiumTests != null && newTestRequest.ApplicationUnderTest != null)
            {
                
                if (!Directory.Exists(testRequestFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(testRequestFolder);
                        Directory.CreateDirectory(Path.Combine(testRequestFolder, "Appium"));
                        Directory.CreateDirectory(Path.Combine(testRequestFolder, "APK"));
                        newTestRequest.AppiumTests.CopyTo(new FileStream(Path.Combine(testRequestFolder, "Appium", newTestRequest.AppiumTests.FileName), FileMode.Create));
                        newTestRequest.ApplicationUnderTest.CopyTo(new FileStream(Path.Combine(testRequestFolder, "APK", newTestRequest.ApplicationUnderTest.FileName), FileMode.Create));
                    }
                    catch (Exception ex)
                    {
                        return new TestRequestWebCreateResult { Success = false, Message = ex.ToString() };
                    }
                }
                else
                {
                    return new TestRequestWebCreateResult { Success = false, Message = $"Directory with Test Request ID {testRequestID} already exists!" };
                }
            }
            else {
                return new TestRequestWebCreateResult { Success = false, Message = "AppiumTests or/and APK are null"};
            }

            //IF SUCCESSFULL, CALL API LAYER WITH NEW TEST REQUEST AND PATHS TO THE UPLOADED FILES

            var AppiumPath = Path.Combine(testRequestFolder, "Appium", newTestRequest.AppiumTests.FileName);
            var ApplicationUnderTestPath = Path.Combine(testRequestFolder, "APK", newTestRequest.ApplicationUnderTest.FileName);

            TestRequestCreateDTO testRequestCreateDTO = new TestRequestCreateDTO
            {
                AppiumTests = AppiumPath,
                ApplicationUnderTest = ApplicationUnderTestPath
            };

            var response = await _client.PostAsJsonAsync(_baseAddress + "/create", testRequestCreateDTO);

            return new TestRequestWebCreateResult { Success = true };

        }

        private string GetUniqueTestRequestID()
        {
            return DateTime.UtcNow.ToString("yyyy_MM_dd_hh_mm_ss")
                      + "_"
                      + Guid.NewGuid().ToString().Substring(0, 4);
        }

        public async Task<TestRequest> UpdateTestRequest(string id, TestRequest updatedTestRequest)
        {
            var response = await _client.PutAsJsonAsync(_baseAddress + "/update", updatedTestRequest);

            return await response.ReadContentAsync<TestRequest>();
        }
    }
}
