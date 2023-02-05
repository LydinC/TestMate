using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium.Support.UI;
using TestMate.Common.DataTransferObjects.APIResponse;
using TestMate.Common.DataTransferObjects.Developers;
using TestMate.Common.DataTransferObjects.TestRequests;
using TestMate.Common.Models.Developers;
using TestMate.Common.Models.TestRequests;
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

        //[Route("TestRequests/{id}")]
        //public async Task<IActionResult> Details(string id)
        //{
        //    var testRequest = await _service.GetTestRequestDetails(id);

        //    return View(testRequest);
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

            _logger.LogInformation("Called Create method");
            var response = await _client.PostAsJsonAsync<TestRequestWebCreateDTO>(_client.BaseAddress + "/Create", testRequestWebCreateDTO);
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



        //[Route("TestRequests/Edit/{id}")]
        //public async Task<IActionResult> Edit(string id, TestRequest updatedTestRequest)
        //{
        //    var testRequest = await _service.UpdateTestRequest(id, updatedTestRequest);
        //    return View(testRequest);
        //}
    }
}
