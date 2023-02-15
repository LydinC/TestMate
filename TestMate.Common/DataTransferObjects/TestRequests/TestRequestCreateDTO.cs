using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using TestMate.Common.Models.TestRequests;

namespace TestMate.Common.DataTransferObjects.TestRequests
{
    public class TestRequestCreateDTO
    {
        [Required]
        public Guid RequestId { get; set; }

        [Required(ErrorMessage = "Application Under Test (APK) is required")]
        public string ApplicationUnderTestPath { get; set; } = null!;

        [Required(ErrorMessage = "Test Solution Path is required")]
        public string TestSolutionPath { get; set; } = null!;

        [Required(ErrorMessage = "Desired Device Properties are required")]
        public DesiredDeviceProperties DesiredDeviceProperties { get; set; } = null!;

        //[Required(ErrorMessage = "Context Configurations are required")]
        //public string ContextConfiguration { get; set; } = null!;

        public TestRequestCreateDTO(Guid requestId, string applicationUnderTestPath, string testSolutionPath, DesiredDeviceProperties desiredDeviceProperties)
        {
            RequestId = requestId;
            ApplicationUnderTestPath = applicationUnderTestPath;
            TestSolutionPath = testSolutionPath;
            DesiredDeviceProperties = desiredDeviceProperties;
            //ContextConfiguration = contextConfiguration;
        }
    }
}
