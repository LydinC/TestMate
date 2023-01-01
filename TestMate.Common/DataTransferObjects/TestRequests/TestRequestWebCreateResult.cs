using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMate.Common.DataTransferObjects.TestRequests
{
    public class TestRequestWebCreateResult
    {
        public Boolean Success { get; set; }
        public string Message { get; set; }
    }
}
