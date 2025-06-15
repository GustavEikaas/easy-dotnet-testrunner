using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace EasyDotnet.Services;

public class MsBuildService
{

  public BuildResult RequestRestore(string targetPath)
  {
    using var pc = new ProjectCollection();
    var buildRequest = new BuildRequestData(targetPath, null, null, ["Restore"], null);
    var logger = new InMemoryLogger();

    var parameters = new BuildParameters(pc) { Loggers = [logger] };

    var result = BuildManager.DefaultBuildManager.Build(parameters, buildRequest);

    return new BuildResult(Success: result.OverallResult == BuildResultCode.Success, result, logger.Messages);
  }

  public BuildResult RequestBuild(string targetPath, string configuration)
  {
    var properties = new Dictionary<string, string?> { { "Configuration", configuration } };

    using var pc = new ProjectCollection();
    var buildRequest = new BuildRequestData(targetPath, properties, null, ["Restore", "Build"], null);
    var logger = new InMemoryLogger();

    var parameters = new BuildParameters(pc) { Loggers = [logger] };

    var result = BuildManager.DefaultBuildManager.Build(parameters, buildRequest);

    return new BuildResult(Success: result.OverallResult == BuildResultCode.Success, result, logger.Messages);
  }

  public DotnetProjectProperties QueryProject(string targetPath, string configuration, string? targetFramework)
  {
    var properties = new Dictionary<string, string> { { "Configuration", configuration } };
    if (!string.IsNullOrEmpty(targetFramework))
    {
      properties.Add("TargetFramework", targetFramework);
    }
    using var pc = new ProjectCollection();

    var project = pc.LoadProject(targetPath);
    project.ReevaluateIfNecessary();

    return new DotnetProjectProperties(
        OutputPath: project.GetPropertyValue("OutputPath"),
        OutputType: project.GetPropertyValue("OutputType"),
        TargetExt: project.GetPropertyValue("TargetExt"),
        AssemblyName: project.GetPropertyValue("AssemblyName"),
        TargetFramework: project.GetPropertyValue("TargetFramework"),
        TargetFrameworks: StringOrNull(project, "TargetFrameworks")?.Split(";"),
        IsTestProject: GetBoolProperty(project, "IsTestProject"),
        UserSecretsId: StringOrNull(project, "UserSecretsId"),
        TestingPlatformDotnetTestSupport: GetBoolProperty(project, "TestingPlatformDotnetTestSupport"),
        TargetPath: project.GetPropertyValue("TargetPath"),
        GeneratePackageOnBuild: GetBoolProperty(project, "GeneratePackageOnBuild"),
        IsPackable: GetBoolProperty(project, "IsPackable"),
        PackageId: project.GetPropertyValue("PackageId"),
        Version: project.GetPropertyValue("Version"),
        PackageOutputPath: project.GetPropertyValue("PackageOutputPath")
    );
  }

  public async Task<bool> AddPackageAsync(string projectPath, string packageId, CancellationToken cancellationToken, string? version = null)
  {
    try
    {
      var arguments = $"add \"{projectPath}\" package {packageId}";

      if (!string.IsNullOrEmpty(version))
      {
        arguments += $" --version {version}";
      }

      var result = await RunDotNetCommandAsync(arguments, cancellationToken);

      if (result.Success)
      {
        return true;
      }
      else
      {
        Console.WriteLine($"Failed to add package {packageId}: {result.Error}");
        return false;
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error adding package {packageId}: {ex.Message}");
      return false;
    }
  }


  private static bool GetBoolProperty(Project project, string name) =>
    string.Equals(project.GetPropertyValue(name), "true", StringComparison.OrdinalIgnoreCase);

  private static string? StringOrNull(Project project, string name)
  {
    var value = project.GetPropertyValue(name);
    return string.IsNullOrWhiteSpace(value) ? null : value;
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

public record BuildResult(bool Success, Microsoft.Build.Execution.BuildResult Result, List<BuildMessage> Messages);


public sealed record BuildMessage(string Type, string FilePath, int LineNumber, int ColumnNumber, string Code, string? Message);

public class InMemoryLogger : ILogger
{
  public List<BuildMessage> Messages { get; } = [];

  public LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Normal;
  public string? Parameters { get; set; }

  public void Initialize(IEventSource eventSource)
  {
    eventSource.ErrorRaised += (sender, args) => Messages.Add(new BuildMessage("error", args.File, args.LineNumber, args.ColumnNumber, args.Code, args?.Message));
    eventSource.WarningRaised += (sender, args) => Messages.Add(new BuildMessage("warning", args.File, args.LineNumber, args.ColumnNumber, args.Code, args?.Message));
  }

  public void Shutdown() { }
}

public sealed record DotnetProjectProperties(
  string OutputPath,
  string? OutputType,
  string? TargetExt,
  string? AssemblyName,
  string? TargetFramework,
  string[]? TargetFrameworks,
  bool IsTestProject,
  string? UserSecretsId,
  bool TestingPlatformDotnetTestSupport,
  string? TargetPath,
  bool GeneratePackageOnBuild,
  bool IsPackable,
  string? PackageId,
  string? Version,
  string? PackageOutputPath
);