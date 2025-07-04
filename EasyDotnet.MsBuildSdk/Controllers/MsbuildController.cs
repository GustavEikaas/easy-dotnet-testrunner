using EasyDotnet.MsBuild.Contracts;
using EasyDotnet.MsBuildSdk.Services;
using Microsoft.Build.Execution;
using StreamJsonRpc;

namespace EasyDotnet.MsBuildSdk.Controllers;

public class MsbuildController(MsBuildService msBuildService)
{
  [JsonRpcMethod("msbuild/build")]
  public MsBuild.Contracts.BuildResult RequestBuild(string targetPath, string configuration)
  {
    var result = msBuildService.RequestBuild(targetPath, configuration);
    return new MsBuild.Contracts.BuildResult(Success: result.BuildResult.OverallResult == BuildResultCode.Success, result.Errors, result.Warnings);
  }

  [JsonRpcMethod("msbuild/query-properties")]
  public DotnetProjectProperties QueryProjectProperties(string targetPath, string configuration, string? targetFramework)
  {
    var props = msBuildService.QueryProject(targetPath, configuration, string.IsNullOrEmpty(targetFramework) ? null : targetFramework);
    return props;
  }

}