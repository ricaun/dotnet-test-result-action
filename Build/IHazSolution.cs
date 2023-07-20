using Nuke.Common;
using Nuke.Common.ProjectModel;

public interface IHazSolution : INukeBuild
{
    [Required][Solution(SuppressBuildProjectCheck = true)] Solution Solution => TryGetValue(() => Solution);
}

