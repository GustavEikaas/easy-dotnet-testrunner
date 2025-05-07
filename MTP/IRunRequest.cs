namespace EasyDotnet.MTP;

public sealed record RunRequest
{
  public string TestExecutablePath { get; init; }
  public RunRequestNode[] Filter { get; init; }
  public string OutFile { get; init; }
}