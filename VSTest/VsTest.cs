using System;
using System.Linq;

namespace EasyDotnet.VSTest;

public static class VsTestHandler
{
  public static void RunDiscover(string vsTestPath, DiscoverProjectRequest[] projects)
  {
    var dllPaths = projects.Select(x => x.dllPath).ToArray();
    var discoveredTests = DiscoverHandler.Discover(vsTestPath, dllPaths);

  projects
        .Join(
            discoveredTests,
            proj => proj.dllPath,
            test => test.Key.Replace("\\","/"),
            (proj, test) => new { proj.outFile, Tests = test.Value}
        )
        .ToList().ForEach(x => TestWriter.WriteDiscoveredTests(x.Tests, x.outFile));
  }

  public static void RunTests(
    string vsTestPath,
    string dllPath,
    Guid[] testIds,
    string outFile
)
  {
    var testResults = RunHandler.RunTests(vsTestPath, dllPath, testIds);
    TestWriter.WriteTestRunResults(testResults, outFile);
  }

}

public sealed record DiscoverProjectRequest(string dllPath, string outFile);