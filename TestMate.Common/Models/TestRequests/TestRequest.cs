using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using TestMate.Common.Enums;

namespace TestMate.Common.Models.TestRequests
{
    public class TestRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        [BsonRequired]
        [BsonRepresentation(BsonType.String)]
        public Guid RequestId { get; set; }

        [Required]
        [BsonRequired]
        public TestRequestStatus Status { get; set; }

        [BsonRequired]
        [Required(ErrorMessage = "Requestor field is required")]
        public string Requestor { get; set; }

        [Required]
        [BsonRequired]
        public DateTime Timestamp { get; set; }

        [Required]
        [BsonRequired]
        public int RetryCount { get; set; }

        [Required]
        [BsonRequired]
        public TestRequestConfiguration Configuration { get; set; }


        public TestRequest(TestRequestConfiguration configuration)
        {
            RequestId = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
            Status = TestRequestStatus.New;
            RetryCount = 0;
            Configuration = configuration;
        }

    }
}
