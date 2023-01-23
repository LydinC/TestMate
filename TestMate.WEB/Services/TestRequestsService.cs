namespace TestMate.WEB.Services
{
    public class TestRequestsService
    {
        private readonly ILogger<TestRequestsService> _logger;
        private readonly string _baseAddress = new Uri("https://localhost:7112/api/testrequests").ToString();

        public TestRequestsService(HttpClient client, ILogger<TestRequestsService> logger)
        {
            _logger = logger;
        }

        //    public async Task<IEnumerable<TestRequest>> GetAllTestRequests()
        //    {
        //        var response = await _client.GetAsync(_baseAddress);

        //        return await response.ReadContentAsync<List<TestRequest>>();
        //    }

        //    public async Task<TestRequest> GetTestRequestDetails(string username)
        //    {
        //        var response = await _client.GetAsync(_baseAddress + "/" + username);

        //        return await response.ReadContentAsync<TestRequest>();
        //    }


        //    public async Task<TestRequestWebCreateResult> CreateTestRequest(TestRequestWebCreateDTO newTestRequest)
        //    {

        //        //TODO: Consider using _webHostEnvironment instead of hardcoded location
        //        string RequestID = Guid.NewGuid().ToString();
        //        var defaultLocation = @"C:/Users/lydin.camilleri/Desktop/Master's Code Repo/Uploads";

        //        var workingFolder = Path.Combine(defaultLocation, RequestID);

        //        if (!IsValidAppiumOptionsJson(newTestRequest.AppiumOptions)) {
        //            _logger.LogError("Invalid AppiumOptions JSON, cannot be deserialized!");
        //            return new TestRequestWebCreateResult { Success = false, Message = $"Appium Options is not a valid JSON!" };
        //        }
        //        if (!IsValidJson(newTestRequest.ContextConfiguration))
        //        {
        //            return new TestRequestWebCreateResult { Success = false, Message = $"Context Configuration is not a valid JSON!" };
        //        }

        //        if (!Directory.Exists(workingFolder))
        //        {
        //            try
        //            {
        //                Directory.CreateDirectory(workingFolder);
        //                Directory.CreateDirectory(Path.Combine(workingFolder, "Test Solution"));
        //                Directory.CreateDirectory(Path.Combine(workingFolder, "Application Under Test"));

        //                var TestSolutionPath = Path.Combine(workingFolder, "Test Solution", newTestRequest.TestSolution.FileName);
        //                var ApplicationUnderTestPath = Path.Combine(workingFolder, "Application Under Test", newTestRequest.ApplicationUnderTest.FileName);

        //                using (var sourceStream = newTestRequest.TestSolution.OpenReadStream())
        //                using (var destinationStream = File.Create(TestSolutionPath))
        //                {
        //                    sourceStream.CopyTo(destinationStream);
        //                }

        //                using (var sourceStream = newTestRequest.ApplicationUnderTest.OpenReadStream())
        //                using (var destinationStream = File.Create(ApplicationUnderTestPath))
        //                {
        //                    sourceStream.CopyTo(destinationStream);
        //                }

        //                ZipFile.ExtractToDirectory(TestSolutionPath, Path.GetDirectoryName(TestSolutionPath));

        //                TestRequestCreateDTO testRequestCreateDTO = new TestRequestCreateDTO
        //                {
        //                    TestSolutionPath = TestSolutionPath,
        //                    ApplicationUnderTestPath = ApplicationUnderTestPath,
        //                    AppiumOptions = newTestRequest.AppiumOptions,
        //                    ContextConfiguration = newTestRequest.ContextConfiguration
        //                };

        //                var response = await _client.PostAsJsonAsync(_baseAddress + "/create", testRequestCreateDTO);

        //                return new TestRequestWebCreateResult { Success = true };

        //            }
        //            catch (Exception ex)
        //            {
        //                return new TestRequestWebCreateResult { Success = false, Message = ex.ToString() };
        //            }
        //        }
        //        else
        //        {
        //            return new TestRequestWebCreateResult { Success = false, Message = $"Directory with Test Request ID {RequestID} already exists!" };
        //        }
        //    }


        //    bool IsValidAppiumOptionsJson(string json)
        //    {
        //        try
        //        {
        //            AppiumOptions appiumOptions = JsonConvert.DeserializeObject<AppiumOptions>(json);
        //            return true;
        //        }
        //        catch (JsonReaderException e)
        //        {
        //            _logger.LogError(e.Message);
        //            return false;
        //        }
        //    }

        //    bool IsValidJson(string json)
        //    {
        //        try
        //        {
        //            JToken.Parse(json);
        //            return true;
        //        }
        //        catch (JsonReaderException e)
        //        {
        //            _logger.LogError("String '{0}' is not a valid json. Exception: {1}", json, e.Message);
        //            return false;
        //        }
        //    }

        //    private string GetUniqueTestRequestID()
        //    {
        //        return DateTime.UtcNow.ToString("yyyy_MM_dd_hh_mm_ss")
        //                  + "_"
        //                  + Guid.NewGuid().ToString().Substring(0, 4);
        //    }

        //    public async Task<TestRequest> UpdateTestRequest(string id, TestRequest updatedTestRequest)
        //    {
        //        var response = await _client.PutAsJsonAsync(_baseAddress + "/update", updatedTestRequest);

        //        return await response.ReadContentAsync<TestRequest>();
        //    }
        //}
    }
}
