using Microsoft.AspNetCore.Mvc;
using TestMate.Common.DataTransferObjects.TestRequests;
using TestMate.Common.Models.TestRequests;
using TestMate.WEB.Services.Interfaces;

namespace TestMate.WEB.Controllers
{
    public class TestRequestsController : Controller
    {
        private readonly ITestRequestsService _service;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<TestRequestsController> _logger;
        public TestRequestsController(ITestRequestsService service, IWebHostEnvironment webHostEnvironment, ILogger<TestRequestsController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;   
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("TestRequests")]
        public async Task<IActionResult> TestRequests()
        {
            var testRequests = await _service.GetAllTestRequests();
            return View(testRequests);
        }

        [Route("TestRequests/{id}")]
        public async Task<IActionResult> DeveloperDetails(string id)
        {
            var testRequest = await _service.GetTestRequestDetails(id);

            return View(testRequest);
        }

        [Route("TestRequests/Create")]
        public IActionResult Create()
        {
            _logger.LogInformation("Navigated to Create");
            return View();
        }


        [HttpPost]
        [Route("TestRequests/Create")]
        public async Task<IActionResult> Create(TestRequestWebCreateDTO testRequestWebCreateDTO)
        {

            _logger.LogInformation("Called Create method");

            if (!ModelState.IsValid) {
                _logger.LogError("Invalid Model!");
                return View(testRequestWebCreateDTO);
            }

            var createResult = await _service.CreateTestRequest(testRequestWebCreateDTO);

            if (!createResult.Success)
            {
                ViewBag.Message = $"TestRequest Submission Failed! {createResult.Message}";
            }
            else 
            {
                ViewBag.Message = "Successfully submitted TestRequest!";
            }

            return View();
        }

        

        [Route("TestRequests/Edit/{id}")]
        public async Task<IActionResult> Edit(string id, TestRequest updatedTestRequest)
        {
            var testRequest = await _service.UpdateTestRequest(id, updatedTestRequest);
            return View(testRequest);
        }
    }
}
