using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyDotnet.MsBuild.Contracts;

namespace EasyDotnet.Services;

public class MsBuildService(IMsBuildHostManager manager)
{
  public async Task<BuildResult> RequestBuildAsync(string targetPath, string configuration, CancellationToken cancellationToken = default)
  {
    //TODO: resolve sdk/framework relation and start appropriate server
    var sdkBuildHost = await manager.GetOrStartClientAsync(BuildClientType.Sdk);
    var result = await sdkBuildHost.BuildAsync(targetPath, configuration);
    Console.WriteLine(string.Join(".", result.Errors));
    return result;
  }
}