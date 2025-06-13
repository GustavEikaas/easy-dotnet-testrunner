using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace EasyDotnet.Services;

public class MsBuildService
{

  public BuildResult RequestBuild(string targetPath, string configuration)
  {
    var properties = new Dictionary<string, string?> { { "Configuration", configuration } };

    var pc = new ProjectCollection(properties);
    var buildRequest = new BuildRequestData(targetPath, properties, null, ["Restore", "Build"], null);
    var logger = new InMemoryLogger();

    var parameters = new BuildParameters(pc) { Loggers = [logger] };

    var result = BuildManager.DefaultBuildManager.Build(parameters, buildRequest);

    return new BuildResult(result, logger.Messages);
  }

  public DotnetProjectProperties QueryProject(string targetPath, string configuration, string? targetFramework)
  {
    var properties = new Dictionary<string, string> { { "Configuration", configuration } };
    if (!string.IsNullOrEmpty(targetFramework))
    {
      properties.Add("TargetFramework", targetFramework);
    }
    var pc = new ProjectCollection(properties);

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

  private static bool GetBoolProperty(Project project, string name) =>
    string.Equals(project.GetPropertyValue(name), "true", StringComparison.OrdinalIgnoreCase);

  private static string? StringOrNull(Project project, string name)
  {
    var value = project.GetPropertyValue(name);
    return string.IsNullOrWhiteSpace(value) ? null : value;
  }
}

public record BuildResult(Microsoft.Build.Execution.BuildResult Result, List<BuildMessage> Messages);


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