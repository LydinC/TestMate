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
        public static FileUploadResult UploadTestRequestFiles(string requestID, IFormFile zippedTestPackage, IFormFile apkFile)
        {
            //TODO: Consider using _webHostEnvironment instead of hardcoded location
            var defaultLocation = @"C:/Users/lydin.camilleri/Desktop/Master's Code Repo/Uploads";
            var workingFolder = Path.Combine(defaultLocation, requestID);

            if (!ValidateFileExtension(zippedTestPackage, new List<string> { ".zip" }))
            {
                return new FileUploadResult("Failed to validate extension for Test Package");
            };

            if (!ValidateFileExtension(apkFile, new List<string> { ".apk" }))
            {
                return new FileUploadResult("Failed to validate extension for ApplicationUnderTest");
            };

            try
            {
                Directory.CreateDirectory(workingFolder);

                //* Test Package *//
                var testPackageDirectory = Path.Combine(workingFolder, "Test Package");
                if (!Directory.Exists(testPackageDirectory))
                {
                    Directory.CreateDirectory(testPackageDirectory);
                }

                var testPackagePath = Path.Combine(testPackageDirectory, zippedTestPackage.FileName);
                using (var sourceStream = zippedTestPackage.OpenReadStream())
                using (var destinationStream = File.Create(testPackagePath))
                {
                    sourceStream.CopyTo(destinationStream);
                }

                ZipFile.ExtractToDirectory(testPackagePath, Path.GetDirectoryName(testPackagePath));
                testPackagePath = Path.Combine(Path.GetDirectoryName(testPackagePath), Path.GetFileNameWithoutExtension(testPackagePath)) ;

                //* Application Under Test *//
                var appUnderTestDirectory = Path.Combine(workingFolder, "Application Under Test");
                if (!Directory.Exists(appUnderTestDirectory))
                {
                    Directory.CreateDirectory(appUnderTestDirectory);
                }

                var apkPath = Path.Combine(appUnderTestDirectory, apkFile.FileName);
                using (var sourceStream = apkFile.OpenReadStream())
                using (var destinationStream = File.Create(apkPath))
                {
                    sourceStream.CopyTo(destinationStream);
                }

                return new FileUploadResult("Files Uploaded Successfully", testPackagePath, apkPath);
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
        public string TestPackagePath { get; set; }
        public string ApkPath { get; set; }

        public FileUploadResult(string message)
        {
            Success = false;
            Message = message;    
        }

        public FileUploadResult(string message, string testPackagePath, string apkPath)
        {
            Success = true;
            Message = message;
            TestPackagePath = testPackagePath;
            ApkPath = apkPath;
        }
    }
}
