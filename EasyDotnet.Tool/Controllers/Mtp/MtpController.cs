using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EasyDotnet.MTP;
using EasyDotnet.Server.Responses;
using EasyDotnet.Services;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.Mtp;

public class MtpController(ClientService clientService) : BaseController
{
  [JsonRpcMethod("mtp/discover")]
  public async Task<FileResultResponse> MtpDiscover(string testExecutablePath, CancellationToken token)
  {
    if (!clientService.IsInitialized)
    {
      throw new Exception("Client has not initialized yet");
    }
    var outFile = Path.GetTempFileName();

    await WithTimeout((token) => MTPHandler.RunDiscoverAsync(testExecutablePath, outFile, token), TimeSpan.FromMinutes(3), token);
    return new(outFile);
  }

  [JsonRpcMethod("mtp/run")]
  public async Task<FileResultResponse> MtpRun(string testExecutablePath, RunRequestNode[] filter, CancellationToken token)
  {
    if (!clientService.IsInitialized)
    {
      throw new Exception("Client has not initialized yet");
    }
    var outFile = Path.GetTempFileName();

    await WithTimeout((token) => MTPHandler.RunTestsAsync(testExecutablePath, filter, outFile, token), TimeSpan.FromMinutes(3), token);
    return new(outFile);
  }
}

