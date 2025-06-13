using System;
using EasyDotnet.Services;
using Microsoft.Build.Execution;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.MsBuild;

public class MsBuildController(ClientService clientService, MsBuildService msBuild, OutFileWriterService outFileWriterService) : BaseController
{

  [JsonRpcMethod("msbuild/build")]
  public BuildResultResponse Build(BuildRequest request)
  {
    if (!clientService.IsInitialized)
    {
      throw new Exception("Client has not initialized yet");
    }

    var buildResult = msBuild.RequestBuild(request.TargetPath, request.ConfigurationOrDefault);

    if (request.OutFile is not null)
    {
      outFileWriterService.WriteBuildResult(buildResult.Messages, request.OutFile);
    }

    return new BuildResultResponse(buildResult.Result.OverallResult == BuildResultCode.Success);
  }
}