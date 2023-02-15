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
        Processing = 1,
        Completed = 2,
        Failed = 3,
        FailedNoDevices = 4
    }
}
