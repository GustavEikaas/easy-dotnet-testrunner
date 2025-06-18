using System.Threading;
using System.Threading.Tasks;
using EasyDotnet.Services;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.Roslyn;

public class RoslynController(RoslynService roslynService) : BaseController
{

  [JsonRpcMethod("roslyn/bootstrap-file")]
  public async Task<BootstrapFileResultResponse> BootstrapFile(string filePath, Kind kind, bool preferFileScopedNamespace)
  {
    var success = await roslynService.BootstrapFile(filePath, kind, preferFileScopedNamespace, new CancellationToken());
    return new(success);
  }

}