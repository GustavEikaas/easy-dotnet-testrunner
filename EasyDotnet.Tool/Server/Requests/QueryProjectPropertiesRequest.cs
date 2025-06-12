namespace EasyDotnet.Server.Requests;

public sealed record QueryProjectPropertiesRequest(
  string TargetPath,
  string? OutFile,
  string? Configuration
)
{
    public string ConfigurationOrDefault => Configuration ?? "Debug";
}
 