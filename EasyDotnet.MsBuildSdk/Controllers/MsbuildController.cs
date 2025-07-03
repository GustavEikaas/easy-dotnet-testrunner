using System.Diagnostics;
using System.Text;
using EasyDotnet.MsBuild.Contracts;
using StreamJsonRpc;

namespace EasyDotnet.MsBuildSdk.Controllers;

public class MsbuildController
{

  [JsonRpcMethod("msbuild/build")]
  public object RequestBuild(string targetPath, string configuration)
  {
    var startInfo = new ProcessStartInfo
    {
      FileName = "msbuild",
      Arguments = $"\"{targetPath}\" /t:Restore;Build /p:Configuration={configuration} /nologo",
      WorkingDirectory = Path.GetDirectoryName(targetPath) ?? Directory.GetCurrentDirectory(),
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false,
      CreateNoWindow = false
    };

    var output = new StringBuilder();
    var errors = new List<string>();
    var warnings = new List<string>();
    var success = false;

    using (var process = new Process { StartInfo = startInfo })
    {
      process.OutputDataReceived += (sender, args) =>
      {
        if (args.Data == null) return;
        output.AppendLine(args.Data);

        if (args.Data.Contains(": error ", StringComparison.OrdinalIgnoreCase))
          errors.Add(args.Data);
        else if (args.Data.Contains(": warning ", StringComparison.OrdinalIgnoreCase))
          warnings.Add(args.Data);
      };

      process.ErrorDataReceived += (sender, args) =>
      {
        if (args.Data == null) return;
        output.AppendLine(args.Data);
        errors.Add(args.Data);
      };

      process.Start();
      process.BeginOutputReadLine();
      process.BeginErrorReadLine();
      process.WaitForExit();

      success = process.ExitCode == 0;
    }


    return new BuildResult(Success: success, errors, warnings);
  }
}

// public class InMemoryLogger : ILogger
// {
//   public List<BuildMessage> Errors { get; } = [];
//   public List<BuildMessage> Warnings { get; } = [];
//
//   public LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Normal;
//   public string? Parameters { get; set; }
//
//   public void Initialize(IEventSource eventSource)
//   {
//     eventSource.ErrorRaised += (sender, args) => Errors.Add(new BuildMessage("error", args.File, args.LineNumber, args.ColumnNumber, args.Code, args?.Message));
//     eventSource.WarningRaised += (sender, args) => Warnings.Add(new BuildMessage("warning", args.File, args.LineNumber, args.ColumnNumber, args.Code, args?.Message));
//   }
//
//   public void Shutdown() { }
// }