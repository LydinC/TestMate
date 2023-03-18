using Microsoft.AspNetCore.Mvc;
using SharpCompress.Common;
using TestMate.Common.DataTransferObjects.APIResponse;
using TestMate.Common.Models.TestRuns;
using TestMate.WEB.Helpers;
using System;
using System.Text;

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

        [Route("TestRuns/Report")]
        public async Task<IActionResult> Report(string Id)
        {

            var requestUri = new Uri(_client.BaseAddress, $"TestRuns/{Id}/HTMLReport");
            var response = await _client.GetAsync(requestUri);
            APIResponse<string> result = await response.ReadContentAsync<APIResponse<string>>();

            if (result.Success)
            {
                object htmlReportContent = (object)result.Data;
                return View(htmlReportContent);
            }
            else
            {
                //TempData["Error"] = result.Message;
                return View();
            }
        }


        [Route("TestRuns/Report/Download")]
        public async Task<IActionResult> Download(string Id)
        {

            var requestUri = new Uri(_client.BaseAddress, $"TestRuns/{Id}/HTMLReport/Download");
            var response = await _client.GetAsync(requestUri);
            APIResponse<string> result = await response.ReadContentAsync<APIResponse<string>>();

            if (result.Success)
            {
                string htmlString = result.Data;
                // Convert the HTML string to a byte array
                byte[] fileBytes = Encoding.UTF8.GetBytes(htmlString);

                // Set the headers to specify that the response is a file download
                Response.ContentType = "application/octet-stream";
                Response.Headers.Add("Content-Disposition", $"attachment; filename=TestMate-{Id}.html");

                // Return the file content as a byte array
                return File(fileBytes, "application/octet-stream");
            }
            else 
            {
                return RedirectToAction("Report", new { Id = Id });
            }

        }
    }
}
