using EasyDotnet.MsBuild.Contracts;

namespace EasyDotnet.MsBuildSdk.Types;

public sealed record RequestBuildResult(Microsoft.Build.Execution.BuildResult BuildResult, List<BuildMessage> Errors, List<BuildMessage> Warnings);