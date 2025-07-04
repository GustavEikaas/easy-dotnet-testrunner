using EasyDotnet.MsBuildSdk.Controllers;
using EasyDotnet.MsBuildSdk.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDotnet.MsBuildSdk;

public static class DiModules
{
  public static IServiceProvider BuildServiceProvider()
  {
    var services = new ServiceCollection();
    services.AddTransient<MsBuildService>();
    services.AddTransient<MsbuildController>();
    services.AddFusionCache();

    return services.BuildServiceProvider();
  }
}