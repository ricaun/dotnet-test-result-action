using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using System.Collections.Generic;
using System.Linq;

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

        var resultFiles = TestResultDirectory.GlobFiles("**\\*.trx");
        foreach (var resultFile in resultFiles)
        {
            var outcomes = GetOutcomes(resultFile).ToList();
            var passedTests = outcomes.Count(x => x == "Passed");
            var failedTests = outcomes.Count(x => x == "Failed");
            var skippedTests = outcomes.Count(x => x == "NotExecuted");

            var resultIcon = (failedTests == 0) ? ":heavy_check_mark:" : ":x:";

            GitHubSummaryWriteLine(
                $"### {resultIcon} Tests `{resultFile.Name}`",
                $"| Passed | Failed | Skipped |",
                $"| ------ | ------ | ------- |",
                $"| {passedTests} | {failedTests} | {skippedTests} |"
                );
        }
    }
}

