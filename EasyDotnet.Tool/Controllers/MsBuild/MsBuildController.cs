using System;
using EasyDotnet.MsBuild.Models;
using EasyDotnet.Server;
using EasyDotnet.Server.Requests;
using EasyDotnet.Server.Responses;
using EasyDotnet.Services;
using Microsoft.Build.Execution;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.MsBuild;

public class MsBuildController(JsonRpc jsonRpc, ClientService clientService, MsBuildService msBuildService) : BaseController
{

  [JsonRpcMethod("msbuild/build")]
  public BuildResultResponse Build(BuildRequest request)
  {
    if (!clientService.IsInitialized)
    {
      throw new Exception("Client has not initialized yet");
    }

    // jsonRpc.NotifyAsync("server/notification", new { message = "Server is initialized!" });
    var buildResult = msBuildService.RequestBuild(request.TargetPath, request.ConfigurationOrDefault);

    if (request.OutFile is not null)
    {
      OutFileWriter.WriteBuildResult(buildResult.Messages, request.OutFile);
    }

    return new(buildResult.Result.OverallResult == BuildResultCode.Success);
  }

  [JsonRpcMethod("msbuild/query-properties")]
  public DotnetProjectPropertiesResponse QueryProperties(QueryProjectPropertiesRequest request)
  {
    if (!clientService.IsInitialized)
    {
      throw new Exception("Client has not initialized yet");
    }

    return msBuildService.QueryProject(request.TargetPath, request.ConfigurationOrDefault).ToResponse();
  }
}