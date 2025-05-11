using System;
using System.Threading.Tasks;
using EasyDotnet.MTP;
using EasyDotnet.VSTest;

using StreamJsonRpc;

namespace EasyDotnet;

internal class Server
{
  [JsonRpcMethod("mtp/discover")]
  public static async Task<string> MtpDiscover(string testExecutablePath, string outFile){
    await MTPHandler.RunDiscoverAsync(testExecutablePath, outFile);
    return outFile;
  }

  [JsonRpcMethod("mtp/run")]
  public static async Task<string> MtpRun(string testExecutablePath, RunRequestNode[] filter, string outFile){
    await MTPHandler.RunTestsAsync(testExecutablePath, filter, outFile);
    return outFile;
  }

  [JsonRpcMethod("vstest/discover")]
  public static string VsTestDiscover(string vsTestPath, DiscoverProjectRequest[] projects)
  {
    VsTestHandler.RunDiscover(vsTestPath, projects);
    return "success";
  }

  [JsonRpcMethod("vstest/run")]
  public static string VsTestRun(string vsTestPath, string dllPath, Guid[] testIds, string outFile)
  {
    VsTestHandler.RunTests(vsTestPath, dllPath, testIds, outFile);
    return outFile;
  }
}
