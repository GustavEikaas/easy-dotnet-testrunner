using EasyDotnet.MsBuild.Contracts;
using EasyDotnet.MsBuildSdk.Types;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using ZiggyCreatures.Caching.Fusion;

namespace EasyDotnet.MsBuildSdk.Services;

public class MsBuildService(IFusionCache cache)
{
  public DotnetProjectProperties QueryProject(string targetPath, string configuration, string? targetFramework)
  {
    var lastWriteTime = File.GetLastWriteTimeUtc(targetPath);
    var cacheKey = $"proj:{targetPath}:{configuration}:{targetFramework ?? "null"}";

    var cached = cache.TryGet<CachedDotnetProjectProperties>(cacheKey);

    if (cached.HasValue && cached.Value.LastModifiedUtc == lastWriteTime)
    {
      return cached.Value.Properties;
    }

    var properties = new Dictionary<string, string> { { "Configuration", configuration } };
    if (!string.IsNullOrEmpty(targetFramework))
    {
      properties.Add("TargetFramework", targetFramework);
    }
    using var pc = new ProjectCollection();

    var project = pc.LoadProject(targetPath);
    project.ReevaluateIfNecessary();

    var result = new DotnetProjectProperties(
        OutputPath: project.GetPropertyValue("OutputPath"),
        OutputType: project.GetPropertyValue("OutputType"),
        TargetExt: project.GetPropertyValue("TargetExt"),
        AssemblyName: project.GetPropertyValue("AssemblyName"),
        TargetFramework: project.GetPropertyValue("TargetFramework"),
        TargetFrameworks: StringOrNull(project, "TargetFrameworks")?.Split(";"),
        IntermediateOutputPath: project.GetPropertyValue("IntermediateOutputPath"),
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

    cache.Set(cacheKey, new CachedDotnetProjectProperties(result, lastWriteTime), TimeSpan.FromMinutes(10));

    return result;
  }

  public RequestBuildResult RequestBuild(string targetPath, string configuration)
  {
    var properties = new Dictionary<string, string?> { { "Configuration", configuration } };

    using var pc = new ProjectCollection();
    var buildRequest = new BuildRequestData(targetPath, properties, null, ["Restore", "Build"], null);
    var logger = new InMemoryLogger();

    var parameters = new BuildParameters(pc) { Loggers = [logger] };

    return new RequestBuildResult(BuildManager.DefaultBuildManager.Build(parameters, buildRequest), logger.Errors, logger.Warnings);
  }

  private static bool GetBoolProperty(Project project, string name) =>
    string.Equals(project.GetPropertyValue(name), "true", StringComparison.OrdinalIgnoreCase);

  private static string? StringOrNull(Project project, string name)
  {
    var value = project.GetPropertyValue(name);
    return string.IsNullOrWhiteSpace(value) ? null : value;
  }

  private sealed record CachedDotnetProjectProperties(
      DotnetProjectProperties Properties,
      DateTime LastModifiedUtc
  );
}

public class InMemoryLogger : ILogger
{
  public List<BuildMessage> Errors { get; } = [];
  public List<BuildMessage> Warnings { get; } = [];

  public LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Normal;
  public string? Parameters { get; set; }

  public void Initialize(IEventSource eventSource)
  {
    eventSource.ErrorRaised += (sender, args) => Errors.Add(new BuildMessage("error", args.File, args.LineNumber, args.ColumnNumber, args.Code, args?.Message));
    eventSource.WarningRaised += (sender, args) => Warnings.Add(new BuildMessage("warning", args.File, args.LineNumber, args.ColumnNumber, args.Code, args?.Message));
  }

  public void Shutdown() { }
}