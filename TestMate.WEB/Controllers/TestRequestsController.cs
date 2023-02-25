using Microsoft.AspNetCore.Mvc;
using TestMate.Common.DataTransferObjects.APIResponse;
using TestMate.Common.DataTransferObjects.TestRequests;
using TestMate.Common.Utils;
using TestMate.WEB.Helpers;
using TestMate.Common.Models.TestRequests;
using Newtonsoft.Json;
using System.IO.Compression;
using SharpCompress.Common;

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

        public IActionResult Index()
        {
            return View();
        }

        //[Route("TestRequests")]
        //public async Task<IActionResult> TestRequests()
        //{
        //    var testRequests = await _client.GetAllTestRequests();
        //    return View(testRequests);
        //}


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
                //TODO: check why providing {"abc": ["samsung"]} still deserializes??????
                DesiredDeviceProperties desiredDeviceProperties = JsonConvert.DeserializeObject<DesiredDeviceProperties>(testRequestWebCreateDTO.DesiredDeviceProperties);
                if (desiredDeviceProperties == null) {
                    throw new Exception("Desired Device Properties cannot be null");
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
                                                                testRequestWebCreateDTO.DesiredContextConfiguration
                                                                )
                    );

                var response = await _client.PostAsJsonAsync<TestRequestCreateDTO>(_client.BaseAddress + "/Create", testRequestCreateDTO);
                var result = await response.ReadContentAsync<APIResponse<TestRequestWebCreateResult>>();
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
                TempData["Error"] = "Something went wrong! Please try again";
                return View();
            }

        }
    }
}
