namespace EasyDotnet.Controllers.Solution;

public sealed record SolutionFileProjectResponse(
    string ProjectName,
    string RelativePath,
    string AbsolutePath
);