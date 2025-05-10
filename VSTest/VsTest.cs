using System.Linq;

namespace EasyDotnet.VSTest;

public static class VsTestHandler
{
  public static void RunDiscover(DiscoverRequest request)
  {
    var dllPaths = request.Projects.Select(x => x.DllPath).ToArray();
    var discoveredTests = DiscoverHandler.Discover(request.VsTestPath, dllPaths);

    request.Projects
        .Join(
            discoveredTests,
            proj => proj.DllPath,
            test => test.Key.Replace("\\","/"),
            (proj, test) => new { proj.OutFile, Tests = test.Value}
        )
        .ToList().ForEach(x => TestWriter.WriteDiscoveredTests(x.Tests, x.OutFile));
  }

  public static void RunTests(RunRequest request)
  {
    var testResults = RunHandler.RunTests(request.VsTestPath, request.DllPath, request.TestIds);
    TestWriter.WriteTestRunResults(testResults, request.OutFile);
  }

}