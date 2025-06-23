using EasyDotnet.IntegrationTests.Utils;
using EasyDotnet.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDotnet.IntegrationTests;

public class ServiceProviderTests
{
  [Fact]
  public void TestServiceProviderHasRequiredServicesForControllers()
  {
    var jsonRpc = RpcTestServerInstantiator.GetUninitializedStreamServer();
    var sp = DiModules.BuildServiceProvider(jsonRpc);
    AssemblyScanner.GetControllerTypes().ForEach(x => sp.GetRequiredService(x));
  }
}