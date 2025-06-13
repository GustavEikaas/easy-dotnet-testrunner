using EasyDotnet.Services;
using EasyDotnet.Utils;
using Microsoft.Extensions.DependencyInjection;
using StreamJsonRpc;

namespace EasyDotnet;

public static class DiModules
{
  //Singleton is scoped per client
  public static ServiceProvider BuildServiceProvider(JsonRpc jsonRpc)
  {
    var services = new ServiceCollection();
    services.AddTransient<MsBuildService>();
    services.AddSingleton<ClientService>();
    services.AddSingleton<OutFileWriterService>();
    services.AddSingleton<VsTestService>();
    services.AddSingleton<MtpService>();
    services.AddSingleton(jsonRpc);
    AssemblyScanner.GetControllerTypes().ForEach(x => services.AddSingleton(x));
    var provider = services.BuildServiceProvider();
    return provider;
  }
}