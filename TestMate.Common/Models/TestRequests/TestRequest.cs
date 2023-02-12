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
        public Guid RequestId { get; set; }

        [Required]
        [BsonRequired]
        public TestRequestStatus Status { get; set; }

        [Required(ErrorMessage = "Requestor field is required")]
        [BsonRequired]
        public string Requestor { get; set; }

        [Required]
        [BsonRequired]
        public DateTime Timestamp { get; set; }

        [Required]
        [BsonRequired]
        public int RetryCount { get; set; }

        [Required]
        [BsonRequired]
        public TestRunConfiguration TestRunConfiguration{ get; set; }


        public TestRequest()
        {
            RequestId = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
            Status = TestRequestStatus.New;
            RetryCount = 0;
        }

    }
}
