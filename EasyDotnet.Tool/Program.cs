using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using EasyDotnet;
using EasyDotnet.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;

class Program
{
  private static readonly string PipeName = "EasyDotnet_450fb6c20d9a401796b86f28217f0345";

  public static async Task<int> Main(string[] args)
  {
    HostDirectoryUtil.HostDirectory = Directory.GetCurrentDirectory();
    if (args.Contains("-v"))
    {
      var assembly = Assembly.GetExecutingAssembly();
      var version = assembly.GetName().Version;
      Console.WriteLine($"Assembly Version: {version}");
      return 0;
    }

    if (args.Contains("--generate-rpc-docs"))
    {
      var doc = RpcDocGenerator.GenerateJsonDoc();
      File.WriteAllText("./rpcDoc.json", doc);
      return 0;
    }

    if (args.Contains("--generate-rpc-docs-md"))
    {
      var md = RpcDocGenerator.GenerateMarkdownDoc().Replace("\r\n", "\n").Replace("\r", "\n");
      File.WriteAllText("./rpcDoc.md", md);
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

  private static async Task RespondToRpcRequestsAsync(Stream stream, int clientId)
  {
    var rpc = JsonRpcServerBuilder.Build(stream, stream);
    rpc.StartListening();
    await rpc.Completion;
    await Console.Error.WriteLineAsync($"Connection #{clientId} terminated.");
  }
}
