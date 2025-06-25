using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using EasyDotnet.MsBuildFramework.Controllers;
using Microsoft.Build.Locator;
using Newtonsoft.Json.Serialization;
using StreamJsonRpc;

namespace EasyDotnet.MsBuildFramework
{
  class Program
  {
    static void BootstrapMsBuild()
    {
      var instance = MSBuildLocator.QueryVisualStudioInstances().First();
      MSBuildLocator.RegisterInstance(instance);
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
        Console.WriteLine($"[CLIENT:{clientId}]Named pipe server started: {pipeName}");
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

      var ts = jsonRpc.TraceSource;
      ts.Switch.Level = SourceLevels.Verbose;
      ts.Listeners.Add(new ConsoleTraceListener());

      jsonRpc.StartListening();
      Console.WriteLine($"JSON-RPC listener attached to #{clientId}. Waiting for requests...");
      await jsonRpc.Completion;
      await Console.Error.WriteLineAsync($"Connection #{clientId} terminated.");
    }
  }
}