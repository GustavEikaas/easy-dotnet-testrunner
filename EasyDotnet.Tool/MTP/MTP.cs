using System.IO;
using System.Threading.Tasks;

namespace EasyDotnet.MTP;

public static class MTPHandler
{
  public static async Task RunDiscoverAsync(string testExecutablePath, string outFile)
  {
    if (!File.Exists(testExecutablePath))
    {
      throw new FileNotFoundException("Test executable not found.", testExecutablePath);
    }
    var tests = await DiscoverHandler.Discover(testExecutablePath);
    TestWriter.WriteDiscoveredTests(tests, outFile);
  }

  public static async Task RunTestsAsync(string testExecutablePath, RunRequestNode[] filter, string outFile)
  {
    var results = await RunHandler.RunTests(testExecutablePath, filter);
    TestWriter.WriteTestRunResults(results, outFile);
  }
}