using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using TestMate.Common.Models.TestRequests;

namespace TestMate.Common.DataTransferObjects.TestRequests
{
    public class TestRequestWebCreateDTO
    {

        [Required(ErrorMessage = "Application Under Test (APK) is required")]
        public IFormFile ApplicationUnderTest { get; set; } = null!;

        [Required(ErrorMessage = "Test Solution is required")]
        public IFormFile TestSolution { get; set; } = null!;

        [Required(ErrorMessage = "Test Solution Executable Name is required")]
        public string SolutionExecutable { get; set; } = null!;

        [Required(ErrorMessage = "Appium Options are required")]
        public DesiredDeviceProperties DesiredDeviceProperties { get; set; } = null!;

        [Required(ErrorMessage = "Context Configurations are required")]
        public string ContextConfiguration { get; set; } = null!;
    }
}
