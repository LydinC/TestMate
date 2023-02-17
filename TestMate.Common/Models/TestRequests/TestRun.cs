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

namespace TestMate.Common.Models.TestRequests
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
        public Dictionary<string, object> DeviceFilter{ get; set; }

        [Required]
        [BsonRequired]
        public string ApplicationUnderTest { get; set; }

        [Required]
        [BsonRequired]
        public string TestSolutionPath { get; set; }

        [Required]
        [BsonRequired]
        public TestRunStatus Status { get; set; }

        [Required]
        [BsonRequired]
        public int RetryCount { get; set; }

        public TestRun(Guid testRequestID, Dictionary<string, object> deviceFilter, string applicationUnderTest, string testSolutionPath)
        {
            TestRequestID = testRequestID;
            DeviceFilter = deviceFilter;
            ApplicationUnderTest = applicationUnderTest;
            TestSolutionPath = testSolutionPath;
            Status = TestRunStatus.New;
            RetryCount = 0;
        }

        public void incrementRetryCount()
        {
            this.RetryCount++;
        }
    }
}
