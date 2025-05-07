namespace EasyDotnet.MTP;

public sealed record DiscoverRequest
{
  public string TestExecutablePath { get; init; }
  public string OutFile { get; init; }
}