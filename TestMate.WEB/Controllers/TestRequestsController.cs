using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TestMate.Common.DataTransferObjects.APIResponse;
using TestMate.Common.DataTransferObjects.TestRequests;
using TestMate.Common.Models.TestRequests;
using TestMate.Common.Utils;
using TestMate.WEB.Helpers;

namespace TestMate.WEB.Controllers
{
    public class TestRequestsController : Controller
    {
        private readonly HttpClient _client;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<TestRequestsController> _logger;

        public TestRequestsController(IHttpClientFactory clientFactory, IWebHostEnvironment webHostEnvironment, ILogger<TestRequestsController> logger)
        {
            _client = clientFactory.CreateClient("TestRequestsClient");
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _client.GetAsync(_client.BaseAddress + "/");
            APIResponse<IEnumerable<TestRequest>> result = await response.ReadContentAsync<APIResponse<IEnumerable<TestRequest>>>();

            if (result.Success)
            {
                return View(result.Data);
            }
            else
            {
                TempData["Error"] = result.Message;
                _logger.LogError(result.Message);
                return View();
            }
            
        }

        [Route("TestRequests/Create")]
        public IActionResult Create()
        {
            _logger.LogInformation("Navigated to TestRequests/Create");
            return View();
        }

        [HttpPost]
        [Route("TestRequests/Create")]
        public async Task<IActionResult> Create(TestRequestWebCreateDTO testRequestWebCreateDTO)
        {
            Guid RequestId = Guid.NewGuid();

            FileUploadResult fileUploadResult = FileUploadUtil.UploadTestRequestFiles(RequestId.ToString(), testRequestWebCreateDTO.TestPackage, testRequestWebCreateDTO.ApplicationUnderTest);
            if (!fileUploadResult.Success){
                TempData["Error"] = fileUploadResult.Message;
                return View();
            } 

            try
            {
                JsonSerializerSettings jsonSettings = new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error };
                DesiredDeviceProperties? desiredDeviceProperties = JsonConvert.DeserializeObject<DesiredDeviceProperties>(testRequestWebCreateDTO.DesiredDeviceProperties, jsonSettings);
                if (desiredDeviceProperties == null) {
                    throw new Exception("Desired Device Properties cannot be null");
                }

                List<DesiredContextConfiguration>? desiredContextConfigurations = new List<DesiredContextConfiguration>();
                if(testRequestWebCreateDTO.DesiredContextConfiguration != null)
                {
                    desiredContextConfigurations = JsonConvert.DeserializeObject<List<DesiredContextConfiguration>>(testRequestWebCreateDTO.DesiredContextConfiguration, jsonSettings);
                }

                //try to find the SolutionExecutable file in the TestSolutionPath
                string[] files = Directory.GetFiles(fileUploadResult.TestPackagePath, testRequestWebCreateDTO.TestExecutableFileNames, SearchOption.AllDirectories);
                if (files.Length == 0)
                {
                    throw new Exception($"Could not find {testRequestWebCreateDTO.TestExecutableFileNames} within {fileUploadResult.TestPackagePath}");
                }
                string TestExecutablePath = files[0];

                TestRequestCreateDTO testRequestCreateDTO = new TestRequestCreateDTO(
                    requestId: RequestId,
                    configuration: new TestRequestConfiguration(fileUploadResult.ApkPath,
                                                                TestExecutablePath,
                                                                desiredDeviceProperties,
                                                                desiredContextConfigurations,
                                                                testRequestWebCreateDTO.PrioritisationStrategy
                                                                )
                    );

                var response = await _client.PostAsJsonAsync<TestRequestCreateDTO>(_client.BaseAddress + "/Create", testRequestCreateDTO);
                var result = await response.ReadContentAsync<APIResponse<TestRequestCreateResultDTO>>();
                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return View();
                }
                else
                {
                    TempData["Error"] = result.Message;
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["Error"] = "Failed to submit Test Request! Please try again! \r\n" + ex.Message;
                return View();
            }
        }


        [Route("TestRequests/Details")]
        public async Task<IActionResult> Details(Guid RequestId)
        {
            var requestUri = new Uri(_client.BaseAddress, $"TestRequests/Details?RequestId={RequestId}");
            var response = await _client.GetAsync(requestUri);
            APIResponse<TestRequest> result = await response.ReadContentAsync<APIResponse<TestRequest>>();

            if (result.Success)
            {
                return View(result.Data);
            }
            else
            {
                TempData["Error"] = result.Message;
                return View();
            }
        }


        //[Route("TestRequests")]
        //public async Task<IActionResult> TestRequests()
        //{
        //    var testRequests = await _client.GetAllTestRequests();
        //    return View(testRequests);
        //}

    }
}
