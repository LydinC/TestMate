using System.ComponentModel.DataAnnotations;

namespace TestMate.Common.DataTransferObjects.TestRequests
{
    public class TestRequestCreateDTO
    {

        [Required(ErrorMessage = "Application Under Test (APK) is required")]
        public string ApplicationUnderTestPath { get; set; } = null!;

        [Required(ErrorMessage = "Test Solution Path is required")]
        public string TestSolutionPath { get; set; } = null!;

        [Required(ErrorMessage = "Appium Options are required")]
        public string AppiumOptions { get; set; } = null!;

        [Required(ErrorMessage = "Context Configurations are required")]
        public string ContextConfiguration { get; set; } = null!;

    }
}
