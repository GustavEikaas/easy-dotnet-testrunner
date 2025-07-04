using EasyDotnet.MsBuild.Contracts;
using EasyDotnet.MsBuildSdk.IntegrationTests.Utils;

namespace EasyDotnet.MsBuildSdk.IntegrationTests;

public class QueryProjectPropertiesTests
{
  [Fact]
  public async Task QueryProjectProperties()
  {
    var targetPath = "C:/Users/Gustav/repo/easy-dotnet-server/EasyDotnet.MsBuildSdk.IntegrationTests/EasyDotnet.MsBuildSdk.IntegrationTests.csproj";
    var res = await RpcTestServerInstantiator.InitializedOneShotRequest<DotnetProjectProperties>("msbuild/query-properties", new List<string?>() { targetPath, "Debug", null });
    Assert.NotNull(res);
  }
}