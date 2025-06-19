using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDotnet.Services;

public class MsBuildService
{

  public async Task<bool> RequestBuildAsync(string targetPath, string configuration, CancellationToken cancellationToken = default)
  {
    var args = $"build \"{targetPath}\" -c {configuration}";

    var result = await RunDotNetCommandAsync(args, cancellationToken);

    return result.Success;
  }


  private async Task<CommandResult> RunDotNetCommandAsync(string arguments, CancellationToken cancellationToken)
  {
    var processStartInfo = new ProcessStartInfo
    {
      FileName = "dotnet",
      Arguments = arguments,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false,
      CreateNoWindow = true
    };

    using var process = new Process { StartInfo = processStartInfo };
    process.Start();

    var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
    var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

    await process.WaitForExitAsync(cancellationToken);

    var output = await outputTask;
    var error = await errorTask;

    return new CommandResult
    {
      Success = process.ExitCode == 0,
      Output = output,
      Error = error,
      ExitCode = process.ExitCode
    };
  }

  private class CommandResult
  {
    public bool Success { get; set; }
    public string Output { get; set; }
    public string Error { get; set; }
    public int ExitCode { get; set; }
  }
}