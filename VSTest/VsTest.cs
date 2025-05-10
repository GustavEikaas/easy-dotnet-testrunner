using System.IO;

namespace EasyDotnet.VSTest;

public static class VsTestHandler
{
  public static void RunDiscover(DiscoverRequest request)
  {
    if (!File.Exists(request.DllPath) || !File.Exists(request.VsTestPath))
    {
      throw new System.Exception("File not found");
    }
    var tests = DiscoverHandler.Discover(request.VsTestPath, request.DllPath);
    TestWriter.WriteDiscoveredTests(tests, request.OutFile);
  }

  public static void RunTests(RunRequest request)
  {
    var testResults = RunHandler.RunTests(request.VsTestPath, request.DllPath, request.TestIds);
    TestWriter.WriteTestRunResults(testResults, request.OutFile);
  }

}