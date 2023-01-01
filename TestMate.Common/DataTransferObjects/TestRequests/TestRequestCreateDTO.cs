using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMate.Common.DataTransferObjects.TestRequests
{
    public class TestRequestCreateDTO
    {
        [Required(ErrorMessage = "Application Under Test (APK) is required")]
        public string ApplicationUnderTest { get; set; } = null!;

        [Required(ErrorMessage = "Appium Tests solution is required")]
        public string AppiumTests { get; set; } = null!;

    }
}
