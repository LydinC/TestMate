using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

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

        [Required(ErrorMessage = "Appium Options are required")]
        public string AppiumOptions { get; set; } = null!;

        [Required(ErrorMessage = "Context Configurations are required")]
        public string ContextConfiguration { get; set; } = null!;


        public TestRequestCreateDTO(Guid requestId, string applicationUnderTestPath, string testSolutionPath, string appiumOptions, string contextConfiguration)
        {
            RequestId = requestId;
            ApplicationUnderTestPath = applicationUnderTestPath;
            TestSolutionPath = testSolutionPath;
            AppiumOptions = appiumOptions;
            ContextConfiguration = contextConfiguration;
        }
    }
}
