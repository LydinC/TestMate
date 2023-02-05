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

        [Required(ErrorMessage = "Application Under Test (APK) is required")]
        [BsonRequired]
        public string ApplicationUnderTest { get; set; } = null!;

        [Required(ErrorMessage = "Test Solution Path is required")]
        [BsonRequired]
        public string TestSolutionPath { get; set; } = null!;

        [Required(ErrorMessage = "Appium Options are required")]
        [BsonRequired]
        public string AppiumOptions { get; set; } = null!;

        [Required(ErrorMessage = "Context Configurations are required")]
        [BsonRequired]
        public string ContextConfiguration { get; set; } = null!;

        public TestRequest()
        {
            RequestId = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
            Status = TestRequestStatus.New;
        }

    }
}
