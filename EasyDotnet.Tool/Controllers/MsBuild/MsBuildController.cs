using System.Threading.Tasks;
using EasyDotnet.Services;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.MsBuild;

public class MsBuildController(ClientService clientService, MsBuildService msBuild, OutFileWriterService outFileWriterService, IBuildClientManager manager) : BaseController
{
  [JsonRpcMethod("msbuild/build")]
  public async Task<BuildResultResponse> Build(BuildRequest request)
  {
    var x = await manager.GetOrStartClientAsync(BuildClientType.Sdk);
    var result = await x.BuildAsync(request.TargetPath, request.ConfigurationOrDefault);

    return new(result.Success);
  }
}
