using System;
using System.IO;
using EasyDotnet.Server.Responses;
using EasyDotnet.Services;
using EasyDotnet.VSTest;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.VsTest;

public class VsTestController(ClientService clientService) : BaseController
{

  [JsonRpcMethod("vstest/discover")]
  public FileResultResponse VsTestDiscover(string vsTestPath, string dllPath)
  {
    if (!clientService.IsInitialized)
    {
      throw new Exception("Client has not initialized yet");
    }
    var outFile = Path.GetTempFileName();

    VsTestHandler.RunDiscover(vsTestPath, [new DiscoverProjectRequest(dllPath, outFile)]);
    return new(outFile);
  }

  [JsonRpcMethod("vstest/run")]
  public FileResultResponse VsTestRun(string vsTestPath, string dllPath, Guid[] testIds)
  {
    if (!clientService.IsInitialized)
    {
      throw new Exception("Client has not initialized yet");
    }
    var outFile = Path.GetTempFileName();

    VsTestHandler.RunTests(vsTestPath, dllPath, testIds, outFile);
    return new(outFile);
  }
}