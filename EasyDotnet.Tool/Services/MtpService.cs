using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyDotnet.Extensions;
using EasyDotnet.MTP;
using EasyDotnet.MTP.RPC;

namespace EasyDotnet.Services;

public class MtpService(OutFileWriterService outFileWriterService)
{
  public async Task RunDiscoverAsync(string testExecutablePath, string outFile, CancellationToken token)
  {
    if (!File.Exists(testExecutablePath))
    {
      throw new FileNotFoundException("Test executable not found.", testExecutablePath);
    }

    await using var client = await Client.CreateAsync(testExecutablePath);
    var discovered = await client.DiscoverTestsAsync(token);
    var tests = discovered.Where(x => x != null && x.Node != null).Select(x => x.ToDiscoveredTest()).ToList();
    outFileWriterService.WriteDiscoveredTests(tests, outFile);
  }

  public async Task RunTestsAsync(string testExecutablePath, RunRequestNode[] filter, string outFile, CancellationToken token)
  {

    if (!File.Exists(testExecutablePath))
    {
      throw new FileNotFoundException("Test executable not found.", testExecutablePath);
    }
    await using var client = await Client.CreateAsync(testExecutablePath);
    var runResults = await client.RunTestsAsync(filter, token);
    var results = runResults.Select(x => x.ToTestRunResult()).ToList();
    outFileWriterService.WriteTestRunResults(results, outFile);
  }
}