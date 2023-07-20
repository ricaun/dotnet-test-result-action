using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;

public interface IHazIGitHubActions
{
    GitHubActions GitHubActions => GitHubActions.Instance;

    void GitHubSummaryWriteLine(params object[] messages)
    {
        foreach (var message in messages)
        {
            Serilog.Log.Information($"StepSummaryFile: {message}");
            GitHubActions?.StepSummaryFile.AppendAllText($"{message}\n");
        }
    }
}

