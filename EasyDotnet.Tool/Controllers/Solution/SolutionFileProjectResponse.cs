using Microsoft.Build.Construction;

namespace EasyDotnet.Controllers.Solution;

public sealed record SolutionFileProjectResponse(
    string ProjectName,
    string RelativePath,
    string AbsolutePath
);

public static class SolutionFileProjectExtensions
{
  public static SolutionFileProjectResponse ToResponse(this ProjectInSolution props)
      => new(props.ProjectName, props.RelativePath, props.AbsolutePath);
}