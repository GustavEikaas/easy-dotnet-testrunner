using System.Threading.Tasks;
using EasyDotnet.Services;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.MsBuild;

public class MsBuildController(ClientService clientService, MsBuildService msBuild, OutFileWriterService outFileWriterService) : BaseController
{
  [JsonRpcMethod("msbuild/build")]
  public async Task<BuildResultResponse> Build(BuildRequest request)
  {
    clientService.ThrowIfNotInitialized();

    var buildResult = await msBuild.RequestBuildAsync(request.TargetPath, request.ConfigurationOrDefault);

    return new BuildResultResponse(buildResult);
  }


}