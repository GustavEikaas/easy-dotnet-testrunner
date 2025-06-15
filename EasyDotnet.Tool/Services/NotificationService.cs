using System.Threading.Tasks;
using StreamJsonRpc;

namespace EasyDotnet.Services;

public sealed record ServerRestoreRequest(string TargetPath);

public class NotificationService(JsonRpc jsonRpc)
{
  public async Task RequestRestoreAsync(string targetPath) => await jsonRpc.NotifyWithParameterObjectAsync("request/restore", new ServerRestoreRequest(targetPath));
}