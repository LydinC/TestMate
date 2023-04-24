using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMate.Common.Enums
{
    public enum TestRunStatus
    {
        New = 0,
        InQueue = 1,
        Processing = 2,
        Completed = 3,
        Failed = 4,
        FailedNoDevices = 5
    }
}
