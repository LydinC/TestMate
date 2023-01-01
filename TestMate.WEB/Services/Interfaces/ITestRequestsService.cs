using TestMate.Common.DataTransferObjects.TestRequests;
using TestMate.Common.Models.TestRequests;

namespace TestMate.WEB.Services.Interfaces
{
    public interface ITestRequestsService
    {
        Task<IEnumerable<TestRequest>> GetAllTestRequests();
        Task<TestRequest> GetTestRequestDetails(string id);

        Task<TestRequestWebCreateResult> CreateTestRequest(TestRequestWebCreateDTO newTestRequest);

        Task<TestRequest> UpdateTestRequest(string id, TestRequest updatedTestRequest);

    }
}
