using TestMate.Common.Models.TestRuns;

namespace TestMate.Common.Prioritisation
{
    public class MaxBrandCoveragePrioritiser : ITestRunPrioritiser
    {
        public List<TestRun> Prioritise(List<TestRun> testRuns)
        {
            var brands = testRuns.Select(tr => tr.DeviceFilter["Brand"]).Distinct().ToList();
            List<TestRun> updatedTestRuns = new List<TestRun>();

            int priority = 0; 
            while (testRuns.Count > 0)
            {
                foreach(var brand in brands)
                {
                    TestRun? testRun = testRuns.FirstOrDefault(tr => tr.DeviceFilter["Brand"] == brand);
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
