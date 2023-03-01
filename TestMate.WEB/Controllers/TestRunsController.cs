using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using TestMate.Common.DataTransferObjects.APIResponse;
using TestMate.Common.Models.TestRequests;
using TestMate.Common.Models.TestRuns;
using TestMate.WEB.Helpers;

namespace TestMate.WEB.Controllers
{
    public class TestRunsController : Controller
    {
        private readonly HttpClient _client;
        private readonly ILogger<TestRunsController> _logger;

        public TestRunsController(IHttpClientFactory clientFactory, ILogger<TestRunsController> logger)
        {
            _client = clientFactory.CreateClient("TestRunsClient");
            _logger = logger;
        }

        [Route("TestRuns/List")]
        public async Task<IActionResult> List(Guid RequestId)
        {
            var requestUri = new Uri(_client.BaseAddress, $"TestRuns/List?RequestId={RequestId}");
            var response = await _client.GetAsync(requestUri);
            APIResponse<IEnumerable<TestRun>> result = await response.ReadContentAsync<APIResponse<IEnumerable<TestRun>>>();

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
    }
}
