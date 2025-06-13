using System;
using System.Collections.Generic;
using EasyDotnet.Msbuild;
using EasyDotnet.MsBuild.Models;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;

namespace EasyDotnet.Services;

public class MsBuildService
{
  public Msbuild.Models.BuildResult RequestBuild(string targetPath, string configuration)
  {
    var properties = new Dictionary<string, string?> { { "Configuration", configuration } };

    var pc = new ProjectCollection(properties);
    var buildRequest = new BuildRequestData(targetPath, properties, null, ["Restore", "Build"], null);
    var logger = new InMemoryLogger();

    var parameters = new BuildParameters(pc) { Loggers = [logger] };

    var result = BuildManager.DefaultBuildManager.Build(parameters, buildRequest);

    return new Msbuild.Models.BuildResult(result, logger.Messages);
  }

  public DotnetProjectProperties QueryProject(string targetPath, string configuration)
  {
    var properties = new Dictionary<string, string?> { { "Configuration", configuration } };
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