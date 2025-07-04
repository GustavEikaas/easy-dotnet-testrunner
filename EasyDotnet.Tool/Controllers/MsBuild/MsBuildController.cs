using System.Threading.Tasks;
using EasyDotnet.MsBuild.Contracts;
using EasyDotnet.Services;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.MsBuild;

public class MsBuildController(ClientService clientService, MsBuildService msBuild) : BaseController
{
  [JsonRpcMethod("msbuild/build")]
  public async Task<DotnetProjectProperties> Build(BuildRequest request)
  {
    clientService.ThrowIfNotInitialized();

    var props = await msBuild.QueryProjectPropertiesAsync(request.TargetPath, request.ConfigurationOrDefault, null);

    var result = await msBuild.RequestBuildAsync(request.TargetPath, request.ConfigurationOrDefault);
    return props;

    // return new(result.Success);
  }

  [JsonRpcMethod("msbuild/query-properties")]
  public async Task<DotnetProjectProperties> QueryProjectProperties(BuildRequest request)
  {
    var props = await msBuild.QueryProjectPropertiesAsync(request.TargetPath, request.ConfigurationOrDefault, null);
    return props;
  }
}