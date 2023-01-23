using Newtonsoft.Json;
using OpenQA.Selenium.Appium;

namespace TestMate.Extender
{
    public static class TestMateExtender
    {
        public static int Add(int x, int y)
        {
            return x + y;
        }

        public static bool IsValidAppiumOptionsJson(string json)
        {
            try
            {
                AppiumOptions appiumOptions = JsonConvert.DeserializeObject<AppiumOptions>(json);
                return true;
            }
            catch (JsonReaderException e)
            {
                return false;
            }
        }
    }
}