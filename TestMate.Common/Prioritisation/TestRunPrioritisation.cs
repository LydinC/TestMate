using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestMate.Common.Enums;
using TestMate.Common.Models.TestRuns;

namespace TestMate.Common.Prioritisation
{
    public class TestRunPrioritisation
    {
        private readonly ITestRunPrioritiser Prioritiser;

        public TestRunPrioritisation(TestRunPrioritisationStrategy strategy)
        {
            switch (strategy)
            {
                case TestRunPrioritisationStrategy.Random:
                    Prioritiser = new RandomPrioritiser();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(strategy), "Unhandled Test Run Prioritisation Strategy");
            }
        }

        //TODO: should i update to void?
        public List<TestRun> Prioritise(List<TestRun> testRuns) 
        {
            return Prioritiser.Prioritise(testRuns);
        }
    }
}
