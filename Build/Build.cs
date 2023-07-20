using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

class Build : NukeBuild, ITestReport
{
    public static int Main() => Execute<Build>(e => e.From<ITestReport>().TestReport);
}

public interface ITestReport : IHazIGitHubActions, IHazSolution
{
    Target TestReport => _ => _
        .Executes(() =>
        {
            ReportTestCount();
            ReportTestGitHubSummary();
            ReportTestElement();
        });

    AbsolutePath TestResultDirectory => Solution.Directory;


    void ReportTestCount()
    {
        IEnumerable<string> GetOutcomes(AbsolutePath file)
            => XmlTasks.XmlPeek(
                file,
                "/xn:TestRun/xn:Results/xn:UnitTestResult/@outcome",
                ("xn", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"));

        var resultFiles = TestResultDirectory.GlobFiles("**\\*.trx");
        var outcomes = resultFiles.SelectMany(GetOutcomes).ToList();
        var passedTests = outcomes.Count(x => x == "Passed");
        var failedTests = outcomes.Count(x => x == "Failed");
        var skippedTests = outcomes.Count(x => x == "NotExecuted");

        ReportSummary(_ => _
            .When(failedTests > 0, _ => _
                .AddPair("Failed", failedTests.ToString()))
            .AddPair("Passed", passedTests.ToString())
            .When(skippedTests > 0, _ => _
                .AddPair("Skipped", skippedTests.ToString())));
    }

    void ReportTestGitHubSummary()
    {
        IEnumerable<string> GetOutcomes(AbsolutePath file)
            => XmlTasks.XmlPeek(
                file,
                "/xn:TestRun/xn:Results/xn:UnitTestResult/@outcome",
                ("xn", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"));

        IEnumerable<DateTime> GetStartTimes(AbsolutePath file)
            => XmlTasks.XmlPeek(
                file,
                "/xn:TestRun/xn:Results/xn:UnitTestResult/@startTime",
                ("xn", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")).Select(DateTime.Parse);

        IEnumerable<DateTime> GetEndTimes(AbsolutePath file)
            => XmlTasks.XmlPeek(
                file,
                "/xn:TestRun/xn:Results/xn:UnitTestResult/@endTime",
                ("xn", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")).Select(DateTime.Parse);

        IEnumerable<TimeSpan> GetDurations(AbsolutePath file)
            => XmlTasks.XmlPeek(
                file,
                "/xn:TestRun/xn:Results/xn:UnitTestResult/@duration",
                ("xn", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")).Select(TimeSpan.Parse);


        var resultFiles = TestResultDirectory.GlobFiles("**\\*.trx");

        if (resultFiles.Any())
        {
            GitHubSummaryWriteLine(
                $"|   | Test File | Passed | Failed | Skipped | Total | Time |",
                $"| :-: | --------- | :------: | :------: | :-------: | :-----: | :----: |"
            );

            foreach (var resultFile in resultFiles)
            {
                var outcomes = GetOutcomes(resultFile).ToList();
                var passedTests = outcomes.Count(x => x == "Passed");
                var failedTests = outcomes.Count(x => x == "Failed");
                var skippedTests = outcomes.Count(x => x == "NotExecuted");

                var duration = GetDurations(resultFile).Select(e => e.TotalSeconds).Sum();
                var startTime = GetStartTimes(resultFile).Min(x => x);
                var endTime = GetEndTimes(resultFile).Max(x => x);
                var time = (endTime - startTime).TotalSeconds;

                var resultIcon =
                    (failedTests != 0) ? ":x:" :
                    (skippedTests != 0) ? ":warning:" :
                    ":heavy_check_mark:";

                GitHubSummaryWriteLine(
                    $"| {resultIcon} | {resultFile.Name} | {passedTests} | {failedTests} | {skippedTests} | {outcomes.Count} | {duration:0.00}s |"
                );
            }

            GitHubSummaryWriteLine(
                ""
            );
        }
    }

    void ReportTestElement()
    {
        T DeserializeXml<T>(XElement xElement) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xElement.ToString()))
            {
                return serializer.Deserialize(reader) as T;
            }
        }

        IEnumerable<XElement> GetTestResultElements(AbsolutePath file)
            => XmlTasks.XmlPeekElements(
                file,
                "/xn:TestRun/xn:Results/xn:UnitTestResult",
                ("xn", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"));

        var resultFiles = TestResultDirectory.GlobFiles("**\\*.trx");

        foreach (var resultFile in resultFiles)
        {
            var testResults = GetTestResultElements(resultFile)
                .Select(DeserializeXml<UnitTestResult>)
                .OrderBy(e => e.TestName);

            var testResultIconTitle =
                testResults.Any(e => e.Outcome.Contains("Failed")) ? ":x:" :
                testResults.Any(e => e.Outcome.Contains("NotExecuted")) ? ":warning:" :
                ":heavy_check_mark:";

            var open = testResultIconTitle.Contains(":x:") ? "open" : "";

            GitHubSummaryWriteLine(
                $"<details {open}>",
                "<summary>",
                $"{testResultIconTitle} {resultFile.Name}",
                "</summary>", ""
            );

            GitHubSummaryWriteLine(
                $"|   | TestName | Time | Message |",
                $"| :-: | :--------: | :----: | ------- |"
            );


            foreach (var testResult in testResults)
            {
                //var totalSeconds = (testResult.EndTime - testResult.StartTime).TotalSeconds;
                var totalSeconds = TimeSpan.Parse(testResult.Duration).TotalSeconds;

                var message = $"{testResult.Output?.StdOut}\n{testResult.Output?.StdErr}\n{testResult.Output?.ErrorInfo?.Message}\n{testResult.Output?.ErrorInfo?.StackTrace}";
                message = message.Trim();

                var testResultIcon = testResult.Outcome.Contains("Passed")
                    ? ":heavy_check_mark:"
                    : testResult.Outcome.Contains("NotExecuted") ? ":warning:" : ":x:";

                GitHubSummaryWriteLine(
                    $"| {testResultIcon} | {testResult.TestName} | {totalSeconds:0.00}s | {message.Replace("\r", "").Replace("\n", "<br>")}"
                );
            }

            GitHubSummaryWriteLine(
                ""
            );

            GitHubSummaryWriteLine(
                "</details>", ""
            );

            if (false)
                foreach (var testResult in testResults)
                {
                    var totalSeconds = (testResult.EndTime - testResult.StartTime).TotalSeconds;
                    var message = $"{testResult.Output?.StdOut}\n{testResult.Output?.StdErr}\n{testResult.Output?.ErrorInfo?.Message}\n{testResult.Output?.ErrorInfo?.StackTrace}";
                    message = message.Trim();

                    var testResultIcon = testResult.Outcome.Contains("Passed")
                        ? ":heavy_check_mark:"
                        : testResult.Outcome.Contains("NotExecuted") ? ":warning:" : ":x:";

                    GitHubSummaryWriteLine(
                        $"##### {testResultIcon} \t {testResult.TestName} \t {totalSeconds:0.00}s"
                    );

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        GitHubSummaryWriteLine(
                            $"```",
                            $"{message}",
                            $"```"
                        );
                    }

                    //Serilog.Log.Information($"{testResult.TestName} {testResult.Outcome} {totalSeconds:0.00} | {message}");
                }


        }
    }
}

[XmlRoot("UnitTestResult", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
public class UnitTestResult
{
    [XmlAttribute("executionId")]
    public Guid ExecutionId { get; set; }

    [XmlAttribute("testId")]
    public Guid TestId { get; set; }

    [XmlAttribute("testName")]
    public string TestName { get; set; }

    [XmlAttribute("computerName")]
    public string ComputerName { get; set; }

    [XmlAttribute("duration")]
    public string Duration { get; set; }

    [XmlAttribute("startTime")]
    public DateTime StartTime { get; set; }

    [XmlAttribute("endTime")]
    public DateTime EndTime { get; set; }

    [XmlAttribute("testType")]
    public Guid TestType { get; set; }

    [XmlAttribute("outcome")]
    public string Outcome { get; set; }

    [XmlAttribute("testListId")]
    public Guid TestListId { get; set; }

    [XmlAttribute("relativeResultsDirectory")]
    public string RelativeResultsDirectory { get; set; }

    [XmlElement("Output")]
    public Output Output { get; set; }
}
public class Output
{
    [XmlElement("StdOut")]
    public string StdOut { get; set; }

    [XmlElement("StdErr")]
    public string StdErr { get; set; }

    [XmlElement("ErrorInfo")]
    public ErrorInfo ErrorInfo { get; set; }
}
public class ErrorInfo
{
    [XmlElement("Message")]
    public string Message { get; set; }

    [XmlElement("StackTrace")]
    public string StackTrace { get; set; }
}



