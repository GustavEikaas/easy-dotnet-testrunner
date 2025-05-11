using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading.Tasks;

using EasyDotnet;

using StreamJsonRpc;
class Program
{
  private const string PipeName = "EasyDotnetPipe";

  public static async Task<int> Main(string[] args)
  {
    await StartServerAsync();

    return 0;
  }

  private static async Task StartServerAsync()
  {
    var clientId = 0;
    while (true)
    {
      /* await Console.Error.WriteAsync("Waiting for client to make a connection..."); */

      var stream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
      Console.WriteLine($"Named pipe server started: {PipeName}");
      await stream.WaitForConnectionAsync();
      Task nowait = RespondToRpcRequestsAsync(stream, ++clientId);
    }
  }

  private static async Task RespondToRpcRequestsAsync(NamedPipeServerStream stream, int clientId)
  {
    await Console.Error.WriteLineAsync($"Connection request #{clientId} received. Spinning off an async Task to cater to requests.");
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