using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using EasyDotnet.MTP.RPC.Models;
using EasyDotnet.MTP.RPC.Requests;
using EasyDotnet.MTP.RPC.Response;
using EasyDotnet.Playground.RPC.Requests;

using StreamJsonRpc;

namespace EasyDotnet.MTP.RPC;

public class Client : IAsyncDisposable
{
  private readonly JsonRpc _jsonRpc;
  private readonly TcpClient _tcpClient;
  private readonly IProcessHandle _processHandle;
  private readonly Server _server;

  private Client(JsonRpc jsonRpc, TcpClient tcpClient, IProcessHandle processHandle, Server server)
  {
    _jsonRpc = jsonRpc;
    _tcpClient = tcpClient;
    _processHandle = processHandle;
    _server = server;
  }

  public static async Task<Client> CreateAsync(string testExePath, bool debug = false)
  {
    var tcpListener = new TcpListener(IPAddress.Loopback, 0);
    tcpListener.Start();

    var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
    Console.WriteLine($"Listening on port: {port}");

    var server = new Server();

    var processConfig = new ProcessConfiguration(testExePath)
    {
      Arguments = $"--server --client-host localhost --client-port {port} --diagnostic --diagnostic-verbosity trace",

      OnStandardOutput = (_, output) =>
      {
        if (debug)
        {
          Console.WriteLine(output);
        }
      },
      OnErrorOutput = (_, output) => Console.Error.WriteLine(output),
      OnExit = (_, exitCode) =>
      {
        if (exitCode == 0) return;
        Console.Error.WriteLine($"[{testExePath}]: exit code '{exitCode}'");
      }
    };

    var processHandle = ProcessFactory.Start(processConfig, false);

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
    var tcpClient = await tcpListener.AcceptTcpClientAsync(cts.Token);

    Console.WriteLine("Client connected");

    var stream = tcpClient.GetStream();
    var jsonRpc = new JsonRpc(stream);

    if (debug)
    {
      var ts = jsonRpc.TraceSource;
      ts.Switch.Level = SourceLevels.Verbose;
      ts.Listeners.Add(new ConsoleTraceListener());
    }

    jsonRpc.AddLocalRpcTarget(server, new JsonRpcTargetOptions { MethodNameTransform = CommonMethodNameTransforms.CamelCase });
    jsonRpc.StartListening();

    await jsonRpc.InvokeWithParameterObjectAsync<InitializeResponse>(
      "initialize",
      new InitializeRequest(Environment.ProcessId, new("easy-dotnet"), new(new(DebuggerProvider: false)))
    );

    return new Client(jsonRpc, tcpClient, processHandle, server);
  }

  public async Task<TestNodeUpdate[]> DiscoverTestsAsync()
  {
    var runId = Guid.NewGuid();
    var tcs = new TaskCompletionSource<TestNodeUpdate[]>();
    _server.RegisterResponseListener(runId, tcs);

    await _jsonRpc.InvokeWithParameterObjectAsync<DiscoveryResponse>("testing/discoverTests", new DiscoveryRequest(runId));
    return await tcs.Task ?? throw new Exception("Server didn't respond");
  }

  public async Task<TestNodeUpdate[]> RunTestsAsync(RunRequestNode[] filter)
  {
    var runId = Guid.NewGuid();
    var tcs = new TaskCompletionSource<TestNodeUpdate[]>();
    _server.RegisterResponseListener(runId, tcs);

    await _jsonRpc.InvokeWithParameterObjectAsync<DiscoveryResponse>(
      "testing/runTests",
      new RunRequest(filter, runId)
    );

    var tests = await tcs.Task ?? throw new Exception("Server didn't respond");
    return [.. tests.ToList().Where(x => x.Node.ExecutionState != "in-progress")];
  }

  public async ValueTask DisposeAsync()
  {
    Console.WriteLine("Disposing..." );
    await _jsonRpc.NotifyWithParameterObjectAsync("exit", new object());
    _jsonRpc.Dispose();
    _tcpClient.Dispose();
    _processHandle.WaitForExit();
    _processHandle.Dispose();
    GC.SuppressFinalize(this);
  }

}