using System;
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
  public static async Task<FileResult> MtpDiscover(string testExecutablePath, string outFile)
  {
    await MTPHandler.RunDiscoverAsync(testExecutablePath, outFile);
    return new FileResult(outFile);
  }

  [JsonRpcMethod("mtp/run")]
  public static async Task<FileResult> MtpRun(string testExecutablePath, RunRequestNode[] filter, string outFile)
  {
    await MTPHandler.RunTestsAsync(testExecutablePath, filter, outFile);
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
}