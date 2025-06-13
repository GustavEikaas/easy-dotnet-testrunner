using EasyDotnet.Services;

namespace EasyDotnet.Controllers.MsBuild;

public sealed record DotnetProjectPropertiesResponse(
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

public static class DotnetProjectPropertiesExtensions
{
  public static DotnetProjectPropertiesResponse ToResponse(this DotnetProjectProperties props)
      => new(
          OutputPath: props.OutputPath,
          OutputType: props.OutputType,
          TargetExt: props.TargetExt,
          AssemblyName: props.AssemblyName,
          TargetFramework: props.TargetFramework,
          TargetFrameworks: props.TargetFrameworks,
          IsTestProject: props.IsTestProject,
          UserSecretsId: props.UserSecretsId,
          TestingPlatformDotnetTestSupport: props.TestingPlatformDotnetTestSupport,
          TargetPath: props.TargetPath,
          GeneratePackageOnBuild: props.GeneratePackageOnBuild,
          IsPackable: props.IsPackable,
          PackageId: props.PackageId,
          Version: props.Version,
          PackageOutputPath: props.PackageOutputPath
      );
}