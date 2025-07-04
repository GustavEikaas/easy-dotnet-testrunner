using System.IO.Pipes;
using Microsoft.Build.Locator;

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
    var rpc = JsonRpcServerBuilder.Build(stream, stream);
    rpc.StartListening();
    await rpc.Completion;
    await Console.Error.WriteLineAsync($"Connection #{clientId} terminated.");
  }
}