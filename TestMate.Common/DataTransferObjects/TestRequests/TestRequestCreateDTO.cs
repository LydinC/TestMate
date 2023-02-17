using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using TestMate.Common.Models.TestRequests;

namespace TestMate.Common.DataTransferObjects.TestRequests
{
    public class TestRequestCreateDTO
    {
        [Required]
        public Guid RequestId { get; set; }

        [Required(ErrorMessage = "TestRequestConfiguration is required!")]
        public TestRequestConfiguration Configuration { get; set; }

        public TestRequestCreateDTO(Guid requestId, TestRequestConfiguration configuration)
        {
            RequestId = requestId;
            Configuration = configuration;
        }
    }
}
