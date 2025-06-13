using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDotnet.Controllers;

public class BaseController
{

  public static Task WithTimeout(Func<CancellationToken, Task> func, TimeSpan timeout, CancellationToken callerToken) =>
    WithTimeout<object>(
      async ct =>
      {
        await func(ct);
        return null!;
      },
      timeout,
      callerToken
    );

  public static async Task<T> WithTimeout<T>(Func<CancellationToken, Task<T>> func, TimeSpan timeout, CancellationToken callerToken)
  {
    using var timeoutCts = new CancellationTokenSource(timeout);
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(callerToken, timeoutCts.Token);

    return await func(linkedCts.Token);
  }
}
