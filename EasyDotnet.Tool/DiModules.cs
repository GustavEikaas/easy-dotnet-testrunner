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
    services.AddSingleton(jsonRpc);
    services.AddSingleton<ClientService>();

    services.AddTransient<MsBuildService>();
    services.AddTransient<NotificationService>();
    services.AddTransient<NugetService>();
    services.AddTransient<OutFileWriterService>();
    services.AddTransient<VsTestService>();
    services.AddTransient<MtpService>();

    AssemblyScanner.GetControllerTypes().ForEach(x => services.AddTransient(x));

    return services.BuildServiceProvider();
  }
}