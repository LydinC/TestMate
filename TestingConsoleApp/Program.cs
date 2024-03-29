﻿using System.Collections.Generic;
using System.Linq;
using System.Xml.Xsl;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using System.Xml.Serialization;


public class Program
{
    public static void Main(string[] args)
    {
        // Load the input XML file
        XmlDocument doc = new XmlDocument();
        doc.Load("C:\\Users\\lydin.camilleri\\Desktop\\NUnitResult.xml");

        //XmlSerializer serializer = new XmlSerializer(typeof(TestRun));
        //TestRun testRun;
        //using (TextReader reader = new StringReader(doc.InnerXml))
        //{
        //    testRun = (TestRun)serializer.Deserialize(reader);
        //}
       // Load the XSLT file for transforming the XML into HTML
        XslCompiledTransform xslt = new XslCompiledTransform();
        xslt.Load("C:\\Users\\lydin.camilleri\\Desktop\\Master's Code Repo\\TestMate\\TestingConsoleApp\\custom.xslt");


        // Create the output HTML file
        using (XmlWriter writer = XmlWriter.Create("C:\\Users\\lydin.camilleri\\Desktop\\outputnew2.html", new XmlWriterSettings() { Indent = true }))
        {
            // Transform the XML into HTML and write it to the output file
            xslt.Transform(doc, writer);
        }
    }




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

    /*
    static void Main(string[] args)
    {
        // Specify the paths to the input XML file and the output HTML file
        string inputFilePath = "C:\\Users\\lydin.camilleri\\Desktop\\NUnitResult.xml";
        string outputFilePath = "C:\\Users\\lydin.camilleri\\Desktop\\output.html";

        // Load the input XML file into an XDocument object
        XDocument doc = XDocument.Load(inputFilePath);

        // Create the output HTML file
        using (StreamWriter writer = new StreamWriter(outputFilePath))
        {
            // Write the HTML header and body start tags
            writer.WriteLine("<html>");
            writer.WriteLine("<head>");
            writer.WriteLine("<title>NUnit Test Results</title>");
            writer.WriteLine("<style>");
            writer.WriteLine("table { border-collapse: collapse; width: 100%; }");
            writer.WriteLine("td, th { border: 1px solid #dddddd; text-align: left; padding: 8px; }");
            writer.WriteLine("tr:nth-child(even) { background-color: #dddddd; }");
            writer.WriteLine("</style>");
            writer.WriteLine("</head>");
            writer.WriteLine("<body>");

            // Write the test suite name and duration
            XElement suiteElement = doc.Descendants("test-suite").First();
            string suiteName = suiteElement.Attribute("name").Value;
            double suiteTime = double.Parse(suiteElement.Attribute("duration").Value);
            writer.WriteLine($"<h1>{suiteName}</h1>");
            writer.WriteLine($"<p>Duration: {suiteTime} seconds</p>");

            // Write the test cases and their details
            IEnumerable<XElement> caseElements = doc.Descendants("test-case");
            foreach (XElement caseElement in caseElements)
            {
                string caseName = caseElement.Attribute("name").Value;
                string caseResult = caseElement.Attribute("result").Value;
                double caseTime = double.Parse(caseElement.Attribute("duration").Value);
                writer.WriteLine("<h2>{0} - {1} ({2} seconds)</h2>", caseName, caseResult, caseTime);

                // Write the failure details if the test case failed
                XElement failureElement = caseElement.Descendants("failure").FirstOrDefault();
                if (failureElement != null)
                {
                    string failureMessage = failureElement.Value;
                    //string failureStackTrace = failureElement.Element("stack-trace").Value;
                    writer.WriteLine("<p><strong>Failure Message:</strong> {0}</p>", failureMessage);
                    //writer.WriteLine("<p><strong>Stack Trace:</strong></p>");
                    //writer.WriteLine("<pre>{0}</pre>", failureStackTrace);
                }

                // Write the output details if there is any output
                XElement outputElement = caseElement.Descendants("output").FirstOrDefault();
                if (outputElement != null)
                {
                    string outputText = outputElement.Value.Trim();
                    if (!string.IsNullOrEmpty(outputText))
                    {
                        writer.WriteLine("<p><strong>Output:</strong></p>");
                        writer.WriteLine("<pre>{0}</pre>", outputText);
                    }
                }
            }

            // Write the HTML body end tag
            writer.WriteLine("</body>");
            writer.WriteLine("</html>");
        }

        Console.WriteLine("Conversion complete");
    }
    */
}