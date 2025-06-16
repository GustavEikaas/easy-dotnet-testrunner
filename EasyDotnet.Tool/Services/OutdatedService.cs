using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetOutdated.Core;
using DotNetOutdated.Core.Models;
using DotNetOutdated.Core.Services;
using NuGet.Versioning;

namespace EasyDotnet.Services;

public class OutdatedService(IProjectAnalysisService projectAnalysisService, INuGetPackageResolutionService nugetService)
{

  public async Task<List<DependencyInfo>> AnalyzeProjectDependenciesAsync(
              string projectPath,
              bool includeTransitive = false,
              int transitiveDepth = 1,
              bool includeUpToDate = false,
              PrereleaseReporting prereleaseReporting = PrereleaseReporting.Auto,
              VersionLock versionLock = VersionLock.None,
              string runtime = "")
  {
    var result = new List<DependencyInfo>();

    // Analyze the project
    var projects = await projectAnalysisService.AnalyzeProjectAsync(
        projectPath,
        false,
        includeTransitive,
        transitiveDepth,
        runtime);

    foreach (var project in projects)
    {
      foreach (var targetFramework in project.TargetFrameworks)
      {
        var dependencies = targetFramework.Dependencies.Values
            .OrderBy(d => d.IsTransitive)
            .ThenBy(d => d.Name);

        foreach (var dependency in dependencies)
        {
          var dependencyInfo = await AnalyzeDependencyAsync(
              project,
              targetFramework,
              dependency,
              prereleaseReporting,
              versionLock,
              includeUpToDate);

          if (dependencyInfo != null)
          {
            result.Add(dependencyInfo);
          }
        }
      }
    }

    return result;
  }

  private async Task<DependencyInfo?> AnalyzeDependencyAsync(
      Project project,
      TargetFramework targetFramework,
      Dependency dependency,
      PrereleaseReporting prereleaseReporting,
      VersionLock versionLock,
      bool includeUpToDate)
  {
    var referencedVersion = dependency.ResolvedVersion;
    NuGetVersion? latestVersion = null;

    if (referencedVersion != null)
    {
      latestVersion = await nugetService.ResolvePackageVersions(
          dependency.Name,
          referencedVersion,
          project.Sources,
          dependency.VersionRange,
          versionLock,
          prereleaseReporting,
          string.Empty, // prereleaseLabel
          targetFramework.Name,
          project.FilePath,
          dependency.IsDevelopmentDependency,
          0, // olderThanDays
          false); // ignoreFailedSources
    }

    var isOutdated = referencedVersion != null &&
                      latestVersion != null &&
                      referencedVersion != latestVersion;

    // Only include if outdated or if includeUpToDate is true
    if (!isOutdated && !includeUpToDate)
    {
      return null;
    }

    return new DependencyInfo
    {
      Name = dependency.Name,
      CurrentVersion = referencedVersion?.ToString() ?? "Unknown",
      LatestVersion = latestVersion?.ToString() ?? "Unknown",
      TargetFramework = targetFramework.Name.ToString(),
      IsOutdated = isOutdated,
      IsTransitive = dependency.IsTransitive,
      UpgradeSeverity = GetUpgradeSeverity(referencedVersion, latestVersion)
    };
  }

  private static string GetUpgradeSeverity(NuGetVersion? current, NuGetVersion? latest)
  {
    if (current == null || latest == null || current.Equals(latest))
    {
      return "None";
    }

    if (current.Major != latest.Major)
    {
      return "Major";
    }

    if (current.Minor != latest.Minor)
    {
      return "Minor";
    }

    if (current.Patch != latest.Patch)
    {
      return "Patch";
    }

    return "Unknown";
  }

  public class DependencyInfo
  {
    public required string Name { get; init; }
    public required string CurrentVersion { get; init; }
    public required string LatestVersion { get; init; }
    public required string TargetFramework { get; init; }
    public required bool IsOutdated { get; init; }
    public required bool IsTransitive { get; init; }
    public required string UpgradeSeverity { get; init; }
  }
}
