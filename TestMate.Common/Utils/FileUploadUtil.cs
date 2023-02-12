using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMate.Common.Utils
{
    public class FileUploadUtil
    {
        public static FileUploadResult UploadTestRequestFiles(string requestID, IFormFile testSolutionPackage, IFormFile applicationUnderTest)
        {
            //TODO: Consider using _webHostEnvironment instead of hardcoded location
            var defaultLocation = @"C:/Users/lydin.camilleri/Desktop/Master's Code Repo/Uploads";
            var workingFolder = Path.Combine(defaultLocation, requestID);

            if (!ValidateFileExtension(testSolutionPackage, new List<string> { ".zip" }))
            {
                return new FileUploadResult("Failed to validate extension for TestSolutionPackage");
            };

            if (!ValidateFileExtension(applicationUnderTest, new List<string> { ".apk" }))
            {
                return new FileUploadResult("Failed to validate extension for ApplicationUnderTest");
            };

            try
            {
                Directory.CreateDirectory(workingFolder);

                //* Test Solution *//
                var testSolutionDirectory = Path.Combine(workingFolder, "Test Solution");
                if (!Directory.Exists(testSolutionDirectory))
                {
                    Directory.CreateDirectory(testSolutionDirectory);
                }

                var testSolutionPath = Path.Combine(testSolutionDirectory, testSolutionPackage.FileName);
                using (var sourceStream = testSolutionPackage.OpenReadStream())
                using (var destinationStream = File.Create(testSolutionPath))
                {
                    sourceStream.CopyTo(destinationStream);
                }

                ZipFile.ExtractToDirectory(testSolutionPath, Path.GetDirectoryName(testSolutionPath));


                //* Application Under Test *//
                var appUnderTestDirectory = Path.Combine(workingFolder, "Application Under Test");
                if (!Directory.Exists(appUnderTestDirectory))
                {
                    Directory.CreateDirectory(appUnderTestDirectory);
                }

                var appUnderTestPath = Path.Combine(appUnderTestDirectory, applicationUnderTest.FileName);
                using (var sourceStream = applicationUnderTest.OpenReadStream())
                using (var destinationStream = File.Create(appUnderTestPath))
                {
                    sourceStream.CopyTo(destinationStream);
                }

                return new FileUploadResult("Files Uploaded Successfully", testSolutionPath, appUnderTestPath);
            }
            catch (Exception ex)
            {
                return new FileUploadResult("Failed to upload files! " + ex.Message);
            }
        }

        private static bool ValidateFileExtension(IFormFile file, List<string> extensions)
        {
            foreach (string extension in extensions)
            {
                if (file.FileName.EndsWith(extension))
                {
                    return true;
                }
            }
            return false;
        }


    }

    public class FileUploadResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string TestSolutionPath { get; set; }
        public string ApplicationUnderTestPath { get; set; }


        public FileUploadResult(string message)
        {
            Success = false;
            Message = message;
        }

        public FileUploadResult(string message, string testSolutionPath, string applicationUnderTestPath)
        {
            Success = true;
            Message = message;
            TestSolutionPath = testSolutionPath;
            ApplicationUnderTestPath = applicationUnderTestPath;
        }
    }
}
