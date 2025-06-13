namespace EasyDotnet.Controllers.MsBuild;

public sealed record QueryProjectPropertiesRequest(
  string TargetPath,
  string? OutFile,
  string? Configuration,
  string? TargetFramework
)
{
  public string ConfigurationOrDefault => Configuration ?? "Debug";
}