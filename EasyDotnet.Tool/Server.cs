using System;
using System.Diagnostics;
using System.IO;
using EasyDotnet.Utils;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using StreamJsonRpc;

namespace EasyDotnet;

public static class JsonRpcServerBuilder
{
  public static JsonRpc Build(Stream writer, Stream reader, Func<JsonRpc, ServiceProvider>? buildServiceProvider = null)
  {
    var formatter = CreateJsonMessageFormatter();
    var handler = new HeaderDelimitedMessageHandler(writer, reader, formatter);
    var jsonRpc = new JsonRpc(handler);

    var sp = buildServiceProvider is not null ? buildServiceProvider(jsonRpc) : DiModules.BuildServiceProvider(jsonRpc);
    RegisterControllers(jsonRpc, sp);
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

  private static void RegisterControllers(JsonRpc jsonRpc, IServiceProvider provider)
  {
    foreach (var controllerType in AssemblyScanner.GetControllerTypes())
    {
      try
      {
        var instance = provider.GetRequiredService(controllerType);
        jsonRpc.AddLocalRpcTarget(instance);
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine($"Failed to add RPC target for {controllerType.FullName}: {ex.Message}");
      }
    }
  }

  private static void EnableTracingIfNeeded(JsonRpc jsonRpc)
  {
    if (Environment.GetEnvironmentVariable("ENABLE_JSONRPC_TRACE") == "1")
    {
      var ts = jsonRpc.TraceSource;
      ts.Switch.Level = SourceLevels.Verbose;
      ts.Listeners.Add(new ConsoleTraceListener());
    }
  }
}