using System;
using System.Threading;
using System.Threading.Tasks;

using EasyDotnet.MTP;
using EasyDotnet.VSTest;

using StreamJsonRpc;

namespace EasyDotnet;

#pragma warning disable IDE1006 // Naming Styles
//TODO: figure out how to automatically serialize output
public sealed record FileResult(string outFile);

internal class Server
{
  [JsonRpcMethod("mtp/discover")]
  public static async Task<FileResult> MtpDiscover(string testExecutablePath, string outFile, CancellationToken token)
  {
    await WithTimeout((token) => MTPHandler.RunDiscoverAsync(testExecutablePath, outFile, token), TimeSpan.FromMinutes(3), token);
    return new FileResult(outFile);
  }

  [JsonRpcMethod("mtp/run")]
  public static async Task<FileResult> MtpRun(string testExecutablePath, RunRequestNode[] filter, string outFile, CancellationToken token)
  {
    await WithTimeout((token) => MTPHandler.RunTestsAsync(testExecutablePath, filter, outFile, token), TimeSpan.FromMinutes(3), token);
    return new FileResult(outFile);
  }

  [JsonRpcMethod("vstest/discover")]
  public static string VsTestDiscover(string vsTestPath, DiscoverProjectRequest[] projects)
  {
    VsTestHandler.RunDiscover(vsTestPath, projects);
    return "success";
  }

  [JsonRpcMethod("vstest/run")]
  public static FileResult VsTestRun(string vsTestPath, string dllPath, Guid[] testIds, string outFile)
  {
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