using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using EasyDotnet.MTP.RPC.Models;
using EasyDotnet.MTP.RPC.Requests;
using EasyDotnet.MTP.RPC.Response;
using StreamJsonRpc;


namespace EasyDotnet.MTP.RPC;

public class MtpServerFactory : IDisposable
{
  private static MtpServerFactory? s_instance = null;
  public readonly TcpListener Listener;

  private MtpServerFactory(TcpListener listener) => Listener = listener;

  public static MtpServerFactory CreateOrGet()
  {
    Console.WriteLine("Creating instance");
    if (s_instance is not null)
    {
      return s_instance;
    }
    var tcpListener = new TcpListener(IPAddress.Loopback, 0);
    tcpListener.Start();
    s_instance = new MtpServerFactory(tcpListener);
    return s_instance;
  }

  public async Task<MtpServerSender> WaitForClientConnect()
  {
    var reciever = new MtpServerReciever();

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
    var tcpClient = await Listener.AcceptTcpClientAsync();

    var stream = tcpClient.GetStream();
    var jsonRpc = new JsonRpc(stream);

    jsonRpc.AddLocalRpcTarget(reciever, new JsonRpcTargetOptions { MethodNameTransform = CommonMethodNameTransforms.CamelCase });
    jsonRpc.StartListening();

    await jsonRpc.InvokeWithParameterObjectAsync<InitializeResponse>(
      "initialize",
      new InitializeRequest(Environment.ProcessId, new("easy-dotnet"), new(new(DebuggerProvider: false)))
    );
    return new MtpServerSender(tcpClient, jsonRpc, reciever);
  }

  public void Dispose()
  {
    Listener.Dispose();
    s_instance = null;

    Console.WriteLine("Tcp listener disposed");
    GC.SuppressFinalize(this);
  }
}

public class MtpServerSender(TcpClient tcpClient, JsonRpc jsonRpc, MtpServerReciever reciever) : IAsyncDisposable
{
  public async Task<TestNodeUpdate[]> DiscoverTestsAsync(CancellationToken cancellationToken = default)
  {
    var runId = Guid.NewGuid();

    return await WithCancellation(
           runId,
           () => jsonRpc.InvokeWithParameterObjectAsync<DiscoveryResponse>(
               "testing/discoverTests", new DiscoveryRequest(runId), cancellationToken),
           cancellationToken
       );
  }

  public async ValueTask DisposeAsync()
  {

    await Console.Error.WriteLineAsync("server sender disposed");
    await jsonRpc.NotifyWithParameterObjectAsync("exit", new object());
    jsonRpc.Dispose();
    tcpClient.Dispose();
  }


  public async Task<TestNodeUpdate[]> RunTestsAsync(RunRequestNode[] filter, CancellationToken cancellationToken)
  {
    var runId = Guid.NewGuid();

    var tests = await WithCancellation(
           runId,
           () => jsonRpc.InvokeWithParameterObjectAsync<DiscoveryResponse>(
               "testing/runTests", new RunRequest(filter, runId), cancellationToken),
           cancellationToken
       );

    return [.. tests.Where(x => x.Node.ExecutionState != "in-progress")];
  }

  private async Task<TestNodeUpdate[]> WithCancellation(
      Guid runId,
      Func<Task> invokeRpcAsync,
      CancellationToken cancellationToken)
  {
    var tcs = new TaskCompletionSource<TestNodeUpdate[]>(TaskCreationOptions.RunContinuationsAsynchronously);

    reciever.RegisterResponseListener(runId, tcs);

    using (cancellationToken.Register(() =>
    {
      tcs.TrySetCanceled(cancellationToken);
      reciever.RemoveResponseListener(runId);
    }))
    {
      try
      {
        await invokeRpcAsync();
        return await tcs.Task;
      }
      catch
      {
        reciever.RemoveResponseListener(runId);
        throw;
      }
    }
  }
}