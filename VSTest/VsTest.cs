using System.Linq;

namespace EasyDotnet.VSTest;

public static class VsTestHandler
{
  public static string RunDiscover(DiscoverRequest request)
  {
    var dllPaths = request.Projects.Select(x => x.DllPath).ToArray();
    var discoveredTests = DiscoverHandler.Discover(request.VsTestPath, dllPaths);

    var matchedValues = request.Projects
        .Join(
            discoveredTests,
            proj => proj.DllPath,
            test => test.Key,
            (proj, test) => new { proj.OutFile, Tests = test.Value}
        )
        .ToList();

    matchedValues.ForEach(x => TestWriter.WriteDiscoveredTests(x.Tests, x.OutFile));
    return string.Join(",", discoveredTests.Select(x => x.Key).Distinct().ToList());
  }

  public static void RunTests(RunRequest request)
  {
    var testResults = RunHandler.RunTests(request.VsTestPath, request.DllPath, request.TestIds);
    TestWriter.WriteTestRunResults(testResults, request.OutFile);
  }

}