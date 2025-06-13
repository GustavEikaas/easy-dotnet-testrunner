using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EasyDotnet.MTP;
using EasyDotnet.Server.Requests;
using EasyDotnet.Server.Responses;
using EasyDotnet.VSTest;
using Microsoft.Build.Execution;
using StreamJsonRpc;

namespace EasyDotnet.Server;

public sealed record FileResult(string OutFile);

public sealed record BuildResult(bool Success);

internal class Server(Msbuild.Msbuild msbuild)
{
  private bool IsInitialized { get; set; }

  [JsonRpcMethod("initialize")]
  public InitializeResponse Initialize(InitializeRequest request)
  {
    var assembly = Assembly.GetExecutingAssembly();
    var serverVersion =
      assembly.GetName().Version ?? throw new NullReferenceException("Server version");

    if (!Version.TryParse(request.ClientInfo.Version, out var clientVersion))
    {
      throw new Exception("Invalid client version format");
    }

    if (clientVersion.Major != serverVersion.Major)
    {
      if (clientVersion.Major < serverVersion.Major)
      {
        throw new Exception(
          $"Client is outdated. Please update your client. Server Version: {serverVersion}, Client Version: {clientVersion}"
        );
      }
      else
      {
        throw new Exception(
          $"Server is outdated. Please update the server. `dotnet tool install -g EasyDotnet` Server Version: {serverVersion}, Client Version: {clientVersion}"
        );
      }
    }
    Directory.SetCurrentDirectory(request.ProjectInfo.RootDir);
    IsInitialized = true;
    return new InitializeResponse(new ServerInfo("EasyDotnet", serverVersion.ToString()));
  }

  [JsonRpcMethod("msbuild/build")]
  public BuildResult Build(BuildRequest request)
  {
    if (!IsInitialized)
    {
      throw new Exception("Client has not initialized yet");
    }

    var buildResult = msbuild.RequestBuild(request.TargetPath, request.ConfigurationOrDefault);

    if (request.OutFile is not null)
    {
      OutFileWriter.WriteBuildResult(buildResult.Messages, request.OutFile);
    }

    return new BuildResult(buildResult.Result.OverallResult == BuildResultCode.Success);
  }

  [JsonRpcMethod("mtp/discover")]
  public async Task<FileResult> MtpDiscover(string testExecutablePath, CancellationToken token)
  {
    if (!IsInitialized)
    {
      throw new Exception("Client has not initialized yet");
    }
    var outFile = Path.GetTempFileName();

    await WithTimeout(
      (token) => MTPHandler.RunDiscoverAsync(testExecutablePath, outFile, token),
      TimeSpan.FromMinutes(3),
      token
    );
    return new FileResult(outFile);
  }

  [JsonRpcMethod("mtp/run")]
  public async Task<FileResult> MtpRun(
    string testExecutablePath,
    RunRequestNode[] filter,
    CancellationToken token
  )
  {
    if (!IsInitialized)
    {
      throw new Exception("Client has not initialized yet");
    }
    var outFile = Path.GetTempFileName();

    await WithTimeout(
      (token) => MTPHandler.RunTestsAsync(testExecutablePath, filter, outFile, token),
      TimeSpan.FromMinutes(3),
      token
    );
    return new FileResult(outFile);
  }

  [JsonRpcMethod("vstest/discover")]
  public FileResult VsTestDiscover(string vsTestPath, string dllPath)
  {
    if (!IsInitialized)
    {
      throw new Exception("Client has not initialized yet");
    }
    var outFile = Path.GetTempFileName();

    VsTestHandler.RunDiscover(vsTestPath, [new DiscoverProjectRequest(dllPath, outFile)]);
    return new FileResult(outFile);
  }

  [JsonRpcMethod("vstest/run")]
  public FileResult VsTestRun(string vsTestPath, string dllPath, Guid[] testIds)
  {
    if (!IsInitialized)
    {
      throw new Exception("Client has not initialized yet");
    }
    var outFile = Path.GetTempFileName();

    VsTestHandler.RunTests(vsTestPath, dllPath, testIds, outFile);
    return new FileResult(outFile);
  }

  public static Task WithTimeout(
    Func<CancellationToken, Task> func,
    TimeSpan timeout,
    CancellationToken callerToken
  )
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

  private static async Task<T> WithTimeout<T>(
    Func<CancellationToken, Task<T>> func,
    TimeSpan timeout,
    CancellationToken callerToken
  )
  {
    using var timeoutCts = new CancellationTokenSource(timeout);
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
      callerToken,
      timeoutCts.Token
    );

    return await func(linkedCts.Token);
  }
}
