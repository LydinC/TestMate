using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace TestingConsoleApp
{
    public enum RunState
    {
        NotRunnable,
        Runnable,
        Explicit,
        Skipped,
        Ignored
    }

    public enum TestResult
    {
        Inconclusive,
        Passed,
        Warning,
        Skipped,
        Failed,
        Error
    }

    [Serializable, XmlRoot("test-run", IsNullable = false)]
    public class TestRun
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("fullname")]
        public string Fullname { get; set; }

        [XmlAttribute("runstate")]
        public RunState RunState { get; set; }

        [XmlAttribute("testcasecount")]
        public int TestCaseCount { get; set; }

        [XmlAttribute("result")]
        public TestResult Result { get; set; }

        [XmlAttribute("total")]
        public int Total { get; set; }

        [XmlAttribute("passed")]
        public int Passed { get; set; }

        [XmlAttribute("failed")]
        public int Failed { get; set; }

        [XmlAttribute("inconclusive")]
        public int Inconclusive { get; set; }

        [XmlAttribute("skipped")]
        public int Skipped { get; set; }

        [XmlAttribute("asserts")]
        public int Asserts { get; set; }

        [XmlAttribute("engine-version")]
        public string EngineVersion { get; set; }

        [XmlAttribute("clr-version")]
        public string ClrVersion { get; set; }

        [XmlAttribute("start-time")]
        public string StartTime { get; set; }

        [XmlAttribute("end-time")]
        public string EndTime { get; set; }

        [XmlAttribute("duration")]
        public decimal Duration { get; set; }

        [XmlElement("command-line")]
        public string CommandLine { get; set; }

        [XmlElement("test-suite", typeof(TestSuite))]
        public List<TestSuite> Tests { get; set; }

        [XmlElement("filter")]
        public XmlNode Filter { get; set; }
    }

    [Serializable]
    public class TestBase
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("fullname")]
        public string FullName { get; set; }

        [XmlAttribute("runstate")]
        public RunState RunState { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("result")]
        public TestResult Result { get; set; }

        [XmlAttribute("start-time")]
        public string StartTime { get; set; }

        [XmlAttribute("end-time")]
        public string EndTime { get; set; }

        [XmlAttribute("duration")]
        public decimal Duration { get; set; }
        [XmlAttribute("asserts")]
        public int Asserts { get; set; }

        [XmlArray(ElementName = "properties"), XmlArrayItem("property", typeof(NameValuePair), IsNullable = false)]
        public List<NameValuePair> Properties { get; set; }

        [XmlArray(ElementName = "assertions"), XmlArrayItem("assertion", typeof(TestAssertion), IsNullable = false)]
        public List<TestAssertion> Assertions { get; set; }

        [XmlElement("output")]
        public string Output { get; set; }

        [XmlElement("reason")]
        public TestReason Reason { get; set; }
    }

    public class TestFailure
    {
        [XmlElement("message")]
        public string Message { get; set; }

        [XmlElement("stack-trace")]
        public string StackTrace { get; set; }
    }

    public class TestAssertion
    {
        [XmlAttribute("result")]
        public TestResult Result { get; set; }

        [XmlElement("message")]
        public string Message { get; set; }

        [XmlElement("stack-trace")]
        public string StackTrace { get; set; }
    }

    public class TestReason
    {
        [XmlElement("message")]
        public List<string> Messages { get; set; }
    }

    [Serializable, XmlRoot("test-suite")]
    public class TestSuite : TestBase
    {
        [XmlAttribute("testcasecount")]
        public int TestCaseCount { get; set; }

        [XmlAttribute("total")]
        public int Total { get; set; }

        [XmlAttribute("passed")]
        public int Passed { get; set; }

        [XmlAttribute("failed")]
        public int Failed { get; set; }

        [XmlAttribute("warnings")]
        public int Warnings { get; set; }

        [XmlAttribute("inconclusive")]
        public int Inconclusive { get; set; }

        [XmlAttribute("skipped")]
        public int Skipped { get; set; }

        [XmlElement("environment", typeof(TestEnvironment))]
        public TestEnvironment Environment { get; set; }

        [XmlArray(ElementName = "settings"), XmlArrayItem("setting", typeof(NameValuePair), IsNullable = false)]
        public List<NameValuePair> Settings { get; set; }

        [XmlElement("test-suite", typeof(TestSuite))]
        [XmlElement("test-case", typeof(TestCase))]
        public List<TestBase> Tests { get; set; }
    }

    [Serializable, XmlRoot("test-case")]
    public class TestCase : TestBase
    {
        [XmlAttribute("methodname")]
        public string MethodName { get; set; }

        [XmlAttribute("classname")]
        public string ClassName { get; set; }

        [XmlAttribute("seed")]
        public long Seed { get; set; }

        [XmlAttribute("label")]
        public string Label { get; set; }

        [XmlAttribute("site")]
        public string Site { get; set; } = "Test";
    }

    [Serializable]
    public class TestEnvironment
    {
        [XmlAttribute("framework-version")]
        public string FrameworkVersion { get; set; }

        [XmlAttribute("clr-version")]
        public string ClrVersion { get; set; }

        [XmlAttribute("os-version")]
        public string OsVersion { get; set; }

        [XmlAttribute("platform")]
        public string Platform { get; set; }

        [XmlAttribute("cwd")]
        public string Cwd { get; set; }

        [XmlAttribute("machine-name")]
        public string MachineName { get; set; }

        [XmlAttribute("user")]
        public string User { get; set; }

        [XmlAttribute("user-domain")]
        public string UserDomain { get; set; }

        [XmlAttribute("culture")]
        public string Culture { get; set; }

        [XmlAttribute("uiculture")]
        public string UiCulture { get; set; }

        [XmlAttribute("os-architecture")]
        public string OsArchitecture { get; set; }
    }

    [Serializable]
    public class NameValuePair
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}
