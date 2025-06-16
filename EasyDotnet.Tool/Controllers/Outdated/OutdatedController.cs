using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyDotnet.Services;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.Outdated;

public class OutdatedController(OutdatedService oudatedService) : BaseController
{

  [JsonRpcMethod("outdated/packages")]
  public async Task<List<OutdatedDependencyInfoResponse>> GetOutdatedPackages(string targetPath)
  {
    var dependencies = await oudatedService.AnalyzeProjectDependenciesAsync(
                        targetPath,
                        includeTransitive: true,
                        includeUpToDate: true
                    );
    return [.. dependencies.Select(x => x.ToResponse())];
  }
}
