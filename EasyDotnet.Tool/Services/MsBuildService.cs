using System.Threading;
using System.Threading.Tasks;
using EasyDotnet.Controllers.MsBuild;
using EasyDotnet.MsBuild.Contracts;

namespace EasyDotnet.Services;

public class MsBuildService(IBuildClientManager manager)
{
  public async Task<BuildResult> RequestBuildAsync(string targetPath, string configuration, CancellationToken cancellationToken = default)
  {
    //TODO: resolve sdk/framework relation and start appropriate server
    var x = await manager.GetOrStartClientAsync(BuildClientType.Sdk);
    var result = await x.BuildAsync(targetPath, configuration);
    return result;
  }
}