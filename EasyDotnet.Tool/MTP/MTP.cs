using System.IO;
using System.Linq;
using System.Threading.Tasks;

using EasyDotnet.Playground.RPC;

namespace EasyDotnet.MTP;

public static class MTPHandler
{
  public static async Task RunDiscoverAsync(string testExecutablePath, string outFile)
  {
    if (!File.Exists(testExecutablePath))
    {
      throw new FileNotFoundException("Test executable not found.", testExecutablePath);
    }

    await using var client = await Client.CreateAsync(testExecutablePath);
    var discovered = await client.DiscoverTestsAsync();
    var tests = discovered.Where(x => x != null && x.Node != null).Select(x => x.ToDiscoveredTest()).ToList();
    TestWriter.WriteDiscoveredTests(tests, outFile);
  }

  public static async Task RunTestsAsync(string testExecutablePath, RunRequestNode[] filter, string outFile)
  {
    await using var client = await Client.CreateAsync(testExecutablePath);
    var runResults = await client.RunTestsAsync(filter);
    var results = runResults.Where(x => x.Node.ExecutionState != "in-progress").Select(x => x.ToTestRunResult()).ToList();
    TestWriter.WriteTestRunResults(results, outFile);
  }
}