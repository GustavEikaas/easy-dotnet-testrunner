namespace EasyDotnet.Controllers.MsBuild;

public sealed record BuildRequest(
  string TargetPath,
  string? OutFile,
  string? Configuration
)
{
  public string ConfigurationOrDefault => Configuration ?? "Debug";
}