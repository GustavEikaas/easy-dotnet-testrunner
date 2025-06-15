using System.Threading.Tasks;
using StreamJsonRpc;

namespace EasyDotnet.Services;

// public enum Job
// {
//   restore = 0,
//   build = 1
// }

// public sealed record JobStartNotification(Guid Id, Job Job, string Message, object? Detail = null);
// public sealed record JobEndedNotification(Guid Id, Job Job, string Message, bool Success, string[]? Errors, object? Detail = null);
// public sealed record JobEndedArgument(bool Success, string[]? Errors, object? Detail = null);
public sealed record ServerRestoreRequest(string TargetPath);

public class NotificationService(JsonRpc jsonRpc)
{
  public async Task RequestRestoreAsync(string targetPath) => await jsonRpc.NotifyWithParameterObjectAsync("request/restore", new ServerRestoreRequest(targetPath));

  // public async Task NotifyJobStartAsync(Guid id, Job job, string message, object? detail = null) => await jsonRpc.NotifyWithParameterObjectAsync("job/started", new JobStartNotification(id, job, message, detail));
  // public async Task NotifyJobEndAsync(Guid id, Job job, string message, JobEndedArgument arg) => await jsonRpc.NotifyAsync("job/ended", new JobEndedNotification(id, job, message, arg.Success, arg?.Errors, arg?.Detail));
}