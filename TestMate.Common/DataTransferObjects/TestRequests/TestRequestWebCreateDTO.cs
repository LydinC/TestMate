using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using TestMate.Common.Enums;
using TestMate.Common.Models.TestRequests;

namespace TestMate.Common.DataTransferObjects.TestRequests
{
    public class TestRequestWebCreateDTO
    {

        [Required(ErrorMessage = "Application Under Test (APK) is required")]
        public IFormFile ApplicationUnderTest { get; set; } = null!;

        [Required(ErrorMessage = "Test Package is required")]
        public IFormFile TestPackage { get; set; } = null!;

        [Required(ErrorMessage = "At least one Test Executable File Name is required")]
        public string TestExecutableFileNames { get; set; } = null!;

        [Required(ErrorMessage = "Desired Device Properties are required")]
        public string DesiredDeviceProperties { get; set; } = null!;

        public string? DesiredContextConfiguration { get; set; }

        public TestRunPrioritisationStrategy? PrioritisationStrategy { get; set; }
    }
}
