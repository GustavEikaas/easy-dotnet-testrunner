using System.IO.Pipes;
using EasyDotnet.MsBuildSdk.Controllers;
using Microsoft.Build.Locator;
using Newtonsoft.Json.Serialization;
using StreamJsonRpc;

namespace EasyDotnet.MsBuildSdk;

class Program
{
  static void BootstrapMsBuild()
  {
    MSBuildLocator.AllowQueryAllRuntimeVersions = true;
    MSBuildLocator.RegisterDefaults();
  }

  static async Task Main(string[] args)
  {
    BootstrapMsBuild();
    var pipe = args[0];
    await StartServerAsync(pipe);
  }

  private static async Task StartServerAsync(string pipeName)
  {
    var clientId = 0;
    while (true)
    {
      var stream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
      Console.WriteLine($"Named pipe server started: {pipeName}");
      await stream.WaitForConnectionAsync();
      _ = RespondToRpcRequestsAsync(stream, ++clientId);
    }
  }

  private static async Task RespondToRpcRequestsAsync(Stream stream, int clientId)
  {
    var jsonMessageFormatter = new JsonMessageFormatter();
    jsonMessageFormatter.JsonSerializer.ContractResolver = new DefaultContractResolver
    {
      NamingStrategy = new CamelCaseNamingStrategy(),
    };

    var handler = new HeaderDelimitedMessageHandler(stream, stream, jsonMessageFormatter);
    var jsonRpc = new JsonRpc(handler);
    jsonRpc.AddLocalRpcTarget(new MsbuildController());

    jsonRpc.StartListening();
    Console.WriteLine($"JSON-RPC listener attached to #{clientId}. Waiting for requests...");
    await jsonRpc.Completion;
    await Console.Error.WriteLineAsync($"Connection #{clientId} terminated.");
  }
}