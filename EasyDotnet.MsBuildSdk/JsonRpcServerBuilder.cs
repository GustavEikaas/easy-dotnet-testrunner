using System.Diagnostics;
using EasyDotnet.MsBuildSdk.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using StreamJsonRpc;

namespace EasyDotnet.MsBuildSdk;

public static class JsonRpcServerBuilder
{
  public static JsonRpc Build(Stream writer, Stream reader, Func<JsonRpc, ServiceProvider>? buildServiceProvider = null)
  {
    var formatter = CreateJsonMessageFormatter();
    var handler = new HeaderDelimitedMessageHandler(writer, reader, formatter);
    var jsonRpc = new JsonRpc(handler);

    var sp = buildServiceProvider is not null ? buildServiceProvider(jsonRpc) : DiModules.BuildServiceProvider();
    jsonRpc.AddLocalRpcTarget(sp.GetRequiredService<MsbuildController>());
    EnableTracingIfNeeded(jsonRpc);
    return jsonRpc;
  }

  private static JsonMessageFormatter CreateJsonMessageFormatter() => new()
  {
    JsonSerializer = { ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }}
  };

  private static void EnableTracingIfNeeded(JsonRpc jsonRpc)
  {
#if DEBUG
    var ts = jsonRpc.TraceSource;
    ts.Switch.Level = SourceLevels.Verbose;
    ts.Listeners.Add(new ConsoleTraceListener());
#endif
  }
}