using TestMate.Common.Models.TestRuns;

namespace TestMate.Common.Prioritisation
{
    public class MaxModelCoveragePrioritiser : ITestRunPrioritiser
    {
        public List<TestRun> Prioritise(List<TestRun> testRuns)
        {
            var models = testRuns.Select(tr => tr.DeviceFilter["Model"]).Distinct().ToList();
            List<TestRun> updatedTestRuns = new List<TestRun>();

            int priority = 0; 
            while (testRuns.Count > 0)
            {
                foreach(var model in models)
                {
                    TestRun? testRun = testRuns.FirstOrDefault(tr => tr.DeviceFilter["Model"] == model);
                    if(testRun != null)
                    {
                        testRun.PriorityLevel = priority;
                        updatedTestRuns.Add(testRun);
                        testRuns.Remove(testRun);
                        priority++;
                    }
                }
            }

            return updatedTestRuns;
        }
    }
}
