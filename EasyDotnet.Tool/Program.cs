using System;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using EasyDotnet;
using EasyDotnet.Utils;
using Microsoft.Build.Locator;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json.Serialization;

using StreamJsonRpc;

class Program
{
  private static readonly string PipeName = "EasyDotnet_" + Guid.NewGuid().ToString("N");

  public static async Task<int> Main(string[] args)
  {
    if (args.Contains("-v"))
    {
      var assembly = Assembly.GetExecutingAssembly();
      var version = assembly.GetName().Version;
      Console.WriteLine($"Assembly Version: {version}");
      return 0;
    }
    if (!MSBuildLocator.IsRegistered)
    {
      MSBuildLocator.RegisterDefaults();
    }

    await StartServerAsync();

    return 0;
  }

  private static async Task StartServerAsync()
  {
    var clientId = 0;
    while (true)
    {
      var stream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
      Console.WriteLine($"Named pipe server started: {PipeName}");
      await stream.WaitForConnectionAsync();
      _ = RespondToRpcRequestsAsync(stream, ++clientId);
    }
  }

  private static async Task RespondToRpcRequestsAsync(NamedPipeServerStream stream, int clientId)
  {
    var jsonMessageFormatter = new JsonMessageFormatter();
    jsonMessageFormatter.JsonSerializer.ContractResolver = new DefaultContractResolver
    {
      NamingStrategy = new CamelCaseNamingStrategy(),
    };

    var handler = new HeaderDelimitedMessageHandler(stream, stream, jsonMessageFormatter);
    var jsonRpc = new JsonRpc(handler);
    var provider = DiModules.BuildServiceProvider(jsonRpc);

    AssemblyScanner.GetControllerTypes().ForEach(x =>
    {
      try
      {
        var target = provider.GetRequiredService(x);
        jsonRpc.AddLocalRpcTarget(target);
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine($"Failed to add RPC target for {x.FullName}: {ex.Message}");
      }
    });
    // if (true == true)
    // {
    //   var ts = jsonRpc.TraceSource;
    //   ts.Switch.Level = SourceLevels.Verbose;
    //   ts.Listeners.Add(new ConsoleTraceListener());
    // }
    jsonRpc.StartListening();
    Console.WriteLine($"JSON-RPC listener attached to #{clientId}. Waiting for requests...");
    await jsonRpc.Completion;
    await Console.Error.WriteLineAsync($"Connection #{clientId} terminated.");
  }
}