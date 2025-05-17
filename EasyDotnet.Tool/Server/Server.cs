using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using EasyDotnet.MTP;
using EasyDotnet.Server.Requests;
using EasyDotnet.Server.Responses;
using EasyDotnet.VSTest;

using StreamJsonRpc;

namespace EasyDotnet.Server;

#pragma warning disable IDE1006 // Naming Styles
//TODO: figure out how to automatically serialize output
public sealed record FileResult(string outFile);

internal class Server
{
  private bool isInitialized { get; set; }

  [JsonRpcMethod("initialize")]
  public InitializeResponse Initialize(InitializeRequest request)
  {

    var assembly = Assembly.GetExecutingAssembly();
    var serverVersion = assembly.GetName().Version ?? throw new NullReferenceException("Server version");

    if (!Version.TryParse(request.ClientInfo.Version, out var clientVersion)){
      throw new Exception("Invalid client version format");
    }

    if (clientVersion.Major != serverVersion.Major)
    {
      if (clientVersion.Major < serverVersion.Major)
      {
        throw new Exception($"Client is outdated. Please update your client. Server Version: {serverVersion}, Client Version: {clientVersion}");
      }
      else
      {
        throw new Exception($"Server is outdated. Please update the server. `dotnet tool install -g EasyDotnet` Server Version: {serverVersion}, Client Version: {clientVersion}");
      }
    }
    isInitialized = true;
    return new InitializeResponse(new ServerInfo("EasyDotnet", serverVersion.ToString()));
  }

  [JsonRpcMethod("mtp/discover")]
  public async Task<FileResult> MtpDiscover(string testExecutablePath, string outFile, CancellationToken token)
  {
    if(!isInitialized){
      throw new Exception("Client has not initialized yet");
    }
    
    await WithTimeout((token) => MTPHandler.RunDiscoverAsync(testExecutablePath, outFile, token), TimeSpan.FromMinutes(3), token);
    return new FileResult(outFile);
  }

  [JsonRpcMethod("mtp/run")]
  public async Task<FileResult> MtpRun(string testExecutablePath, RunRequestNode[] filter, string outFile, CancellationToken token)
  {
    if(!isInitialized){
      throw new Exception("Client has not initialized yet");
    }

    await WithTimeout((token) => MTPHandler.RunTestsAsync(testExecutablePath, filter, outFile, token), TimeSpan.FromMinutes(3), token);
    return new FileResult(outFile);
  }

  [JsonRpcMethod("vstest/discover")]
  public string VsTestDiscover(string vsTestPath, DiscoverProjectRequest[] projects)
  {
    if(!isInitialized){
      throw new Exception("Client has not initialized yet");
    }
    
    VsTestHandler.RunDiscover(vsTestPath, projects);
    return "success";
  }

  [JsonRpcMethod("vstest/run")]
  public FileResult VsTestRun(string vsTestPath, string dllPath, Guid[] testIds, string outFile)
  {
    if(!isInitialized){
      throw new Exception("Client has not initialized yet");
    }

    VsTestHandler.RunTests(vsTestPath, dllPath, testIds, outFile);
    return new FileResult(outFile);
  }


  public static Task WithTimeout(
      Func<CancellationToken, Task> func,
      TimeSpan timeout,
      CancellationToken callerToken)
  {
      return WithTimeout<object>(
          async ct => 
          { 
            await func(ct);
            return null!;
          },
          timeout,
          callerToken
      );
  }

  private static async Task<T> WithTimeout<T>(Func<CancellationToken, Task<T>> func,  TimeSpan timeout, CancellationToken callerToken)
  {
    using var timeoutCts = new CancellationTokenSource(timeout);
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(callerToken, timeoutCts.Token);

    return await func(linkedCts.Token);
  }

}