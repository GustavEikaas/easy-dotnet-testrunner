using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

using EasyDotnet.Playground.RPC.Models;
using EasyDotnet.Playground.RPC.Requests;
using EasyDotnet.Playground.RPC.Response;

using StreamJsonRpc;

namespace EasyDotnet.Playground.RPC;

public class Client(string testExePath)
{
  private JsonRpc? _jsonRpc;
  private IProcessHandle? _processHandle;
  private TcpClient? _tcpClient;
  private Server _server = new();

  public async Task Initialize()
  {

    TcpListener tcpListener = new(IPAddress.Loopback, 0);
    tcpListener.Start();
    StringBuilder builder = new();
    Console.WriteLine("Listening on port: " + ((IPEndPoint)tcpListener.LocalEndpoint).Port);
    ProcessConfiguration processConfig = new(testExePath)
    {
      // OnStandardOutput = (_, output) => builder.AppendLine(CultureInfo.InvariantCulture, $"OnStandardOutput:\n{output}"),
      OnStandardOutput = (_, output) => Console.WriteLine(output),
      OnErrorOutput = (_, output) => builder.AppendLine(CultureInfo.InvariantCulture, $"OnErrorOutput:\n{output}"),
      OnExit = (_, exitCode) => builder.AppendLine(CultureInfo.InvariantCulture, $"OnExit: exit code '{exitCode}'"),
      // Arguments = $"--server --client-host localhost --client-port {((IPEndPoint)tcpListener.LocalEndpoint).Port}",
      Arguments = $"--server --client-host localhost --client-port {((IPEndPoint)tcpListener.LocalEndpoint).Port} --diagnostic --diagnostic-verbosity trace",
      // EnvironmentVariables = environmentVariables,
    };

    IProcessHandle processHandle = ProcessFactory.Start(processConfig, cleanDefaultEnvironmentVariableIfCustomAreProvided: false);

    TcpClient tcpClient;
    using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(60));
    try
    {
      tcpClient = await tcpListener.AcceptTcpClientAsync(cancellationTokenSource.Token);
    }
    catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationTokenSource.Token)
    {
      throw new OperationCanceledException($"Timeout on connection for command line '{processConfig.FileName} {processConfig.Arguments}'\n{builder}", ex, cancellationTokenSource.Token);
    }

    if (tcpClient.Connected)
    {
      Console.WriteLine("Client connected");
    }


    var stream = tcpClient.GetStream();
    _jsonRpc = new JsonRpc(stream);
    _tcpClient = tcpClient;
    _processHandle = processHandle;


    if (true == true)
    {
      var ts = _jsonRpc.TraceSource;
      ts.Switch.Level = SourceLevels.Verbose;
      ts.Listeners.Add(new ConsoleTraceListener());
    }


    _jsonRpc.AddLocalRpcTarget(_server,
        new JsonRpcTargetOptions
        {
          MethodNameTransform = CommonMethodNameTransforms.CamelCase,
        }
    );
    _jsonRpc.StartListening();
    var res = await _jsonRpc.InvokeWithParameterObjectAsync<InitializeResponse>("initialize", new InitializeRequest(Environment.ProcessId, new("easy-dotnet"), new(new(DebuggerProvider: false))));
  }

  public async Task<TestNodeUpdate[]> DiscoverTestsAsync()
  {
    if (_jsonRpc is null)
    {
      throw new InvalidOperationException("Initialize must be called first");
    }

    var runId = Guid.NewGuid();
    var discoverTask = new TaskCompletionSource<TestNodeUpdate[]>();
    _server.RegisterResponseListener(runId, discoverTask);

    await _jsonRpc.InvokeWithParameterObjectAsync<DiscoveryResponse>("testing/discoverTests", new DiscoveryRequest(runId));
    var tests = await discoverTask.Task ?? throw new Exception("Server didnt respond??");

    return tests;
  }

  public async Task<TestNodeUpdate[]> RunTestsAsync()
  {
    if (_jsonRpc is null)
    {
      throw new InvalidOperationException("Initialize must be called first");
    }
    var tests = await DiscoverTestsAsync();

    var runId = Guid.NewGuid();
    var runTask = new TaskCompletionSource<TestNodeUpdate[]>();
    _server.RegisterResponseListener(runId, runTask);

    await _jsonRpc.InvokeWithParameterObjectAsync<DiscoveryResponse>("testing/runTests", new RunRequest([.. tests.Select(x => x.Node)], runId));
    var runResult = await runTask.Task ?? throw new Exception("Server didnt respond??");

    return tests;
  }

  public async Task Terminate()
  {
    if (_jsonRpc is null)
    {
      throw new InvalidOperationException("Initialize must be called first");
    }

    await _jsonRpc.NotifyWithParameterObjectAsync("exit", new object());

    _jsonRpc.Dispose();
    _tcpClient?.Dispose();
    _processHandle?.WaitForExit();
    _processHandle?.Dispose();
  }

}
