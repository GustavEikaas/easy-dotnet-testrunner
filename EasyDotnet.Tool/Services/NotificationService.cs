using System.Threading.Tasks;
using EasyDotnet.Notifications;
using StreamJsonRpc;
using static EasyDotnet.Controllers.Roslyn.RoslynController;

namespace EasyDotnet.Services;

public sealed record ServerRestoreRequest(string TargetPath);
//marker interface
public interface INotificationService { }
public class NotificationService(JsonRpc jsonRpc) : INotificationService
{
  [RpcNotification("request/restore")]
  public async Task RequestRestoreAsync(string targetPath) => await jsonRpc.NotifyWithParameterObjectAsync("request/restore", new ServerRestoreRequest(targetPath));

  [RpcNotification("stream/symbol-result")]
  public async Task StreamMessage(SymbolResult symbolResult) => await jsonRpc.NotifyWithParameterObjectAsync("stream/symbol-result", symbolResult);
}