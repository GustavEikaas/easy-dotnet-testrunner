namespace EasyDotnet.MsBuild.Contracts;

public sealed record DotnetProjectProperties(
  string OutputPath,
  string? OutputType,
  string? TargetExt,
  string? AssemblyName,
  string? TargetFramework,
  string[]? TargetFrameworks,
  string? IntermediateOutputPath,
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