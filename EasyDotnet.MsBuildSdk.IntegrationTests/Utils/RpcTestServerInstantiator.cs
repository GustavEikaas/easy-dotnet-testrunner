using Nerdbank.Streams;
using StreamJsonRpc;

namespace EasyDotnet.MsBuildSdk.IntegrationTests.Utils;

public static class RpcTestServerInstantiator
{
  public static JsonRpc GetUninitializedStreamServer()
  {
    var (stream1, stream2) = FullDuplexStream.CreatePair();
    var server = JsonRpcServerBuilder.Build(stream1, stream2);
    server.StartListening();
    return server;
  }

  public static async Task<T> InitializedOneShotRequest<T>(string targetName, object? parameters)
  {
    using var server = GetUninitializedStreamServer();
    return parameters is not null
      ? await server.InvokeWithParameterObjectAsync<T>(targetName, parameters)
      : await server.InvokeAsync<T>(targetName);
  }

}