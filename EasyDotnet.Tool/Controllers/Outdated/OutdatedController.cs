using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyDotnet.Services;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.Outdated;

public class OutdatedController(OutdatedService oudatedService, OutFileWriterService outFileWriterService) : BaseController
{

  [JsonRpcMethod("outdated/packages")]
  public async Task<FileResultResponse> GetOutdatedPackages(string targetPath, bool? includeTransitive = false)
  {
    var dependencies = await oudatedService.AnalyzeProjectDependenciesAsync(
                        targetPath,
                        includeTransitive: includeTransitive ?? false,
                        includeUpToDate: true
                    );

    var outFile = Path.GetTempFileName();
    outFileWriterService.WriteOutdatedDependencies([.. dependencies.Select(x => x.ToResponse())], outFile);

    return new FileResultResponse(outFile);
  }
}