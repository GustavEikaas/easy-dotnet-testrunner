using System;
using System.Threading;
using System.Threading.Tasks;
using EasyDotnet.Services;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.MsBuild;

public class MsBuildController(NotificationService notificationService, ClientService clientService, MsBuildService msBuild, OutFileWriterService outFileWriterService) : BaseController
{
  [JsonRpcMethod("msbuild/build")]
  public BuildResultResponse Build(BuildRequest request)
  {
    clientService.ThrowIfNotInitialized();

    var buildResult = msBuild.RequestBuild(request.TargetPath, request.ConfigurationOrDefault);

    if (request.OutFile is not null)
    {
      outFileWriterService.WriteBuildResult(buildResult.Messages, request.OutFile);
    }

    return new BuildResultResponse(buildResult.Success);
  }

  [JsonRpcMethod("msbuild/restore")]
  public BuildResultResponse Restore(string targetPath)
  {
    clientService.ThrowIfNotInitialized();

    var buildResult = msBuild.RequestRestore(targetPath);

    return new BuildResultResponse(buildResult.Success);
  }

  [JsonRpcMethod("msbuild/pack")]
  public PackResultResponse Pack(string targetPath, string? configuration)
  {
    clientService.ThrowIfNotInitialized();
    var configurationOrDefault = configuration ?? "Release";

    var buildResult = msBuild.RequestPack(targetPath, configurationOrDefault);

    return new PackResultResponse(buildResult.Success, buildResult.FilePath);
  }

  [JsonRpcMethod("msbuild/query-properties")]
  public DotnetProjectPropertiesResponse QueryProperties(QueryProjectPropertiesRequest request)
  {
    clientService.ThrowIfNotInitialized();

    return msBuild.QueryProject(request.TargetPath, request.ConfigurationOrDefault, request.TargetFramework).ToResponse();
  }

  [JsonRpcMethod("msbuild/add-package-reference")]
  public async Task AddPackageReference(string targetPath, string packageName, CancellationToken cancellationToken)
  {
    clientService.ThrowIfNotInitialized();
    var res = await msBuild.AddPackageAsync(targetPath, packageName, cancellationToken);
    if (!res)
    {
      throw new Exception("Failed to add package reference");
    }

    await notificationService.RequestRestoreAsync(targetPath);
  }
}