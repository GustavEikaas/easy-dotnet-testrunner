using EasyDotnet.IntegrationTests.Utils;

namespace EasyDotnet.IntegrationTests.Nuget;

public sealed record TestNugetSourceResponse(string Name, string Uri, bool IsLocal);

public class NugetTests
{

  [Fact]
  public async Task ListSources()
  {
    var res = await RpcTestServerInstantiator.InitializedOneShotRequest<List<TestNugetSourceResponse>>("nuget/list-sources", null);
    Assert.NotNull(res);
    Assert.True(res.Count > 0);
  }
}