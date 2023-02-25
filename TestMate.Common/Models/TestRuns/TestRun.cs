using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using TestMate.Common.Models.Devices;
using TestMate.Common.Enums;

namespace TestMate.Common.Models.TestRuns
{
    public class TestRun
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required]
        [BsonRequired]
        [BsonRepresentation(BsonType.String)]
        public Guid TestRequestID { get; set; }

        [Required]
        [BsonRequired]
        public Dictionary<string, string> DeviceFilter { get; set; }

        [Required]
        [BsonRequired]
        public string ApkPath { get; set; }

        [Required]
        [BsonRequired]
        public string TestExecutablePath { get; set; }

        public Dictionary<string, string>? ContextConfiguration { get; set; }

        [Required]
        [BsonRequired]
        public TestRunStatus Status { get; set; }

        [Required]
        [BsonRequired]
        public int RetryCount { get; set; }

        public TestRun(Guid testRequestID, Dictionary<string, string> deviceFilter, string apkPath, string testExecutablePath, Dictionary<string, string>? contextConfiguration)
        {
            TestRequestID = testRequestID;
            DeviceFilter = deviceFilter;
            ApkPath = apkPath;
            TestExecutablePath = testExecutablePath;
            Status = TestRunStatus.New;
            RetryCount = 0;
            ContextConfiguration = contextConfiguration;
        }

        public void incrementRetryCount()
        {
            RetryCount++;
        }
    }
}
