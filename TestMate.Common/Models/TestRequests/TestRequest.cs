using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace TestMate.Common.Models.TestRequests
{
    public class TestRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required(ErrorMessage = "Requestor field is required")]
        public string Requestor { get; set; } = null!;

        [Required(ErrorMessage = "Timestamp field is required")]
        public DateTime Timestamp { get; set; }

        [Required(ErrorMessage = "Application Under Test (APK) is required")]
        public string ApplicationUnderTest { get; set; } = null!;

        [Required(ErrorMessage = "Appium Tests solution is required")]
        public string AppiumTests { get; set; } = null!;

        [Required(ErrorMessage = "Status field is required")]
        public string Status { get; set; } = null!;
    }
}
