using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestMate.Common.DataTransferObjects.APIResponse;
using TestMate.Common.Enums;
using TestMate.Common.Models.Developers;
using TestMate.Common.Models.TestRuns;

namespace TestMate.Common.Prioritisation
{
    public interface ITestRunPrioritiser
    {
        List<TestRun> Prioritise(List<TestRun> testRuns);
    }
}
