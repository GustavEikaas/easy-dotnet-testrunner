using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EasyDotnet.MTP;
using EasyDotnet.Services;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.Mtp;

public class MtpController(ClientService clientService, MtpService mtpService) : BaseController
{
  [JsonRpcMethod("mtp/discover")]
  public async Task<FileResultResponse> MtpDiscover(string testExecutablePath, CancellationToken token)
  {
    if (!clientService.IsInitialized)
    {
      throw new Exception("Client has not initialized yet");
    }
    var outFile = Path.GetTempFileName();

    await WithTimeout(
      (token) => mtpService.RunDiscoverAsync(testExecutablePath, outFile, token),
      TimeSpan.FromMinutes(3),
      token
    );
    return new FileResultResponse(outFile);
  }

  [JsonRpcMethod("mtp/run")]
  public async Task<FileResultResponse> MtpRun(
    string testExecutablePath,
    RunRequestNode[] filter,
    CancellationToken token
  )
  {
    if (!clientService.IsInitialized)
    {
      throw new Exception("Client has not initialized yet");
    }
    var outFile = Path.GetTempFileName();

    await WithTimeout(
      (token) => mtpService.RunTestsAsync(testExecutablePath, filter, outFile, token),
      TimeSpan.FromMinutes(3),
      token
    );
    return new FileResultResponse(outFile);
  }

  public static Task WithTimeout(
    Func<CancellationToken, Task> func,
    TimeSpan timeout,
    CancellationToken callerToken
  ) => WithTimeout<object>(
      async ct =>
      {
        await func(ct);
        return null!;
      },
      timeout,
      callerToken
    );

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