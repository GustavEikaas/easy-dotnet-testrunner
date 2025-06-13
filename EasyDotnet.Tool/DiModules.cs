using Microsoft.Extensions.DependencyInjection;

namespace EasyDotnet;

public static class DiModules
{
  public static ServiceProvider BuildServiceProvider()
  {
    var services = new ServiceCollection();
    services.AddTransient<Server.Server>();
    services.AddTransient<Msbuild.Msbuild>();
    var provider = services.BuildServiceProvider();
    return provider;
  }
}