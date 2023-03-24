using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestMate.Common.Enums;
using TestMate.Common.Models.TestRuns;

namespace TestMate.Common.Prioritisation
{
    public class RandomPrioritiser : ITestRunPrioritiser
    {
        public List<TestRun> Prioritise(List<TestRun> testRuns) {

            int range = testRuns.Count;
            Random random = new Random();

            //Shuffle the list (Fisher-Yates algorithm)
            for (int i = range - 1; i > 0; i--){
                int j = random.Next(i + 1);
                var temp = testRuns[i];
                testRuns[i] = testRuns[j];
                testRuns[j] = temp;
            }

            for (int i = 0; i < range - 1; i++) {
                testRuns[i].PriorityLevel = i;
            }

            return testRuns;
        }
    }
}
