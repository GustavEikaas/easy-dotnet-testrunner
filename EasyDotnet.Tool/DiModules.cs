using EasyDotnet.Services;
using EasyDotnet.Utils;
using Microsoft.Extensions.DependencyInjection;
using StreamJsonRpc;
using System.Linq;

namespace EasyDotnet;

public static class DiModules
{
  public static ServiceProvider BuildServiceProvider(JsonRpc jsonRpc)
  {
    var services = new ServiceCollection();
    services.AddSingleton<ClientService>();
    services.AddTransient<MsBuildService>();
    services.AddSingleton(x => jsonRpc);
    AssemblyScanner.GetControllerTypes().ToList().ForEach(x => services.AddSingleton(x));
    var provider = services.BuildServiceProvider();
    return provider;
  }
}