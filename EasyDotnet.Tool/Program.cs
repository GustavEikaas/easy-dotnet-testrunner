using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using EasyDotnet;

using StreamJsonRpc;
class Program
{
  private static readonly string PipeName = "EasyDotnetPipe_" + Guid.NewGuid().ToString("N");

  public static async Task<int> Main(string[] args)
  {
    if(args.Contains("-v")){
      var assembly = Assembly.GetExecutingAssembly();
      var version = assembly.GetName().Version; // AssemblyVersion
      Console.WriteLine($"Assembly Version: {version}");
      return 0;
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
    var jsonRpc = JsonRpc.Attach(stream, new Server());
    if(true == true){
      var ts = jsonRpc.TraceSource;
      ts.Switch.Level = SourceLevels.Verbose;
      ts.Listeners.Add(new ConsoleTraceListener());
    }
    jsonRpc.StartListening();
    await Console.Error.WriteLineAsync($"JSON-RPC listener attached to #{clientId}. Waiting for requests...");
    await jsonRpc.Completion;
    await Console.Error.WriteLineAsync($"Connection #{clientId} terminated.");
  }
}