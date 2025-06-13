using System;
using System.IO;
using EasyDotnet.Services;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.VsTest;

public class VsTestController(VsTestService vsTestService, ClientService clientService) : BaseController
{
  [JsonRpcMethod("vstest/discover")]
  public FileResultResponse VsTestDiscover(string vsTestPath, string dllPath)
  {
    if (!clientService.IsInitialized)
    {
      throw new Exception("Client has not initialized yet");
    }
    var outFile = Path.GetTempFileName();

    vsTestService.RunDiscover(vsTestPath, [new DiscoverProjectRequest(dllPath, outFile)]);
    return new FileResultResponse(outFile);
  }

  [JsonRpcMethod("vstest/run")]
  public FileResultResponse VsTestRun(string vsTestPath, string dllPath, Guid[] testIds)
  {
    if (!clientService.IsInitialized)
    {
      throw new Exception("Client has not initialized yet");
    }
    var outFile = Path.GetTempFileName();

    vsTestService.RunTests(vsTestPath, dllPath, testIds, outFile);
    return new FileResultResponse(outFile);
  }
}