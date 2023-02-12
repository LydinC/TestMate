using Microsoft.AspNetCore.Mvc;
using TestMate.Common.DataTransferObjects.APIResponse;
using TestMate.Common.DataTransferObjects.TestRequests;
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


            FileUploadResult fileUploadResult = FileUploadUtil.UploadTestRequestFiles(RequestId.ToString(), testRequestWebCreateDTO.TestSolution, testRequestWebCreateDTO.ApplicationUnderTest);
 
            if (!fileUploadResult.Success){
                TempData["Error"] = fileUploadResult.Message;
                return View();
            } 
            else
            {
                TestRequestCreateDTO testRequestCreateDTO = new TestRequestCreateDTO(
                    requestId: RequestId,
                    applicationUnderTestPath: fileUploadResult.ApplicationUnderTestPath,
                    testSolutionPath: fileUploadResult.TestSolutionPath,
                    appiumOptions: testRequestWebCreateDTO.AppiumOptions,
                    contextConfiguration: testRequestWebCreateDTO.ContextConfiguration
                    );
                

                var response = await _client.PostAsJsonAsync<TestRequestCreateDTO>(_client.BaseAddress + "/Create", testRequestCreateDTO);
                var result = await response.ReadContentAsync<APIResponse<TestRequestWebCreateResult>>();
                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["Error"] = result.Message;
                    return View();
                }

            }
        }
    }
}
