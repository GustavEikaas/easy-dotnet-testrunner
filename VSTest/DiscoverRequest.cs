namespace EasyDotnet.VSTest;

public sealed record DiscoverRequest
{
  public string VsTestPath { get; init; }
  public DiscoverProjectRequest[] Projects {get; init;}
}

public sealed record DiscoverProjectRequest(string DllPath, string OutFile);