using Microsoft.Build.Framework;
using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using EasyDotnet.MTP;
using EasyDotnet.Server.Requests;
using EasyDotnet.Server.Responses;
using EasyDotnet.VSTest;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;

using StreamJsonRpc;

namespace EasyDotnet.Server;

#pragma warning disable IDE1006 // Naming Styles
//TODO: figure out how to automatically serialize output
public sealed record FileResult(string outFile);
public sealed record BuildResult(bool success);

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
    Directory.SetCurrentDirectory(request.ProjectInfo.RootDir);
    isInitialized = true;
    return new InitializeResponse(new ServerInfo("EasyDotnet", serverVersion.ToString()));
  }

  [JsonRpcMethod("msbuild/build")]
  public BuildResult Build(BuildRequest request)
  {
    var properties = new Dictionary<string, string?>
    {
        { "Configuration", request.Configuration}
    };

    var pc = new ProjectCollection(properties);
    var buildRequest = new BuildRequestData(request.TargetPath, properties, null, ["Build"], null);
    var logger = new InMemoryLogger();

    var parameters = new BuildParameters(pc)
    {
        Loggers = [logger]
    };

    var result = BuildManager.DefaultBuildManager.Build(parameters, buildRequest);

    return new BuildResult(result.OverallResult == BuildResultCode.Success);
  }

  [JsonRpcMethod("mtp/discover")]
  public async Task<FileResult> MtpDiscover(string testExecutablePath, CancellationToken token)
  {
    if(!isInitialized){
      throw new Exception("Client has not initialized yet");
    }
    var outFile = Path.GetTempFileName();
    
    await WithTimeout((token) => MTPHandler.RunDiscoverAsync(testExecutablePath, outFile, token), TimeSpan.FromMinutes(3), token);
    return new FileResult(outFile);
  }

  [JsonRpcMethod("mtp/run")]
  public async Task<FileResult> MtpRun(string testExecutablePath, RunRequestNode[] filter, CancellationToken token)
  {
    if(!isInitialized){
      throw new Exception("Client has not initialized yet");
    }
    var outFile = Path.GetTempFileName();

    await WithTimeout((token) => MTPHandler.RunTestsAsync(testExecutablePath, filter, outFile, token), TimeSpan.FromMinutes(3), token);
    return new FileResult(outFile);
  }

  [JsonRpcMethod("vstest/discover")]
  public FileResult VsTestDiscover(string vsTestPath, string dllPath)
  {
    if(!isInitialized){
      throw new Exception("Client has not initialized yet");
    }
    var outFile = Path.GetTempFileName();

    VsTestHandler.RunDiscover(vsTestPath, [new DiscoverProjectRequest(dllPath, outFile)]);
    return new FileResult(outFile);
  }

  [JsonRpcMethod("vstest/run")]
  public FileResult VsTestRun(string vsTestPath, string dllPath, Guid[] testIds)
  {
    if(!isInitialized){
      throw new Exception("Client has not initialized yet");
    }
    var outFile = Path.GetTempFileName();

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

public sealed record BuildError(string FilePath, int LineNumber, int ColumnNumber, string Code, string? Message);

public class InMemoryLogger : ILogger
{
    public List<BuildError> Errors { get; } = [];

    public LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Normal;
    public string? Parameters { get; set; }

    public void Initialize(IEventSource eventSource)
    {
        eventSource.ErrorRaised += (sender, args) => Errors.Add(new BuildError(
                args.File,
                args.LineNumber,
                args.ColumnNumber,
                args.Code,
                args?.Message
            ));
    }

    public void Shutdown() { }
}