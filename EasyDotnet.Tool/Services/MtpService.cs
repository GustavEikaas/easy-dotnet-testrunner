using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using EasyDotnet.Extensions;
using EasyDotnet.MTP;
using EasyDotnet.MTP.RPC;

namespace EasyDotnet.Services;

public class MtpService(OutFileWriterService outFileWriterService)
{
  private static async Task<(MtpServerSender, IProcessHandle)> GetInitializedClient(string testExecutablePath)
  {

    if (!File.Exists(testExecutablePath))
    {
      throw new FileNotFoundException("Test executable not found.", testExecutablePath);
    }

    var server = MtpServerFactory.CreateOrGet();
    var port = ((IPEndPoint)server.Listener.LocalEndpoint).Port;
    Console.WriteLine($"Listening on port: {port}");
    var processConfig = new ProcessConfiguration(testExecutablePath)
    {
      Arguments = $"--server --client-host localhost --client-port {port} --diagnostic --diagnostic-verbosity trace",

      OnStandardOutput = (_, output) => { },
      OnErrorOutput = (_, output) => Console.Error.WriteLine(output),
      OnExit = (_, exitCode) =>
      {

        Console.Error.WriteLine("Process disposed");
        if (exitCode == 0)
        {
          return;
        }
        // Console.Error.WriteLine($"[{testExePath}]: exit code '{exitCode}'");
      }
    };

    var processHandle = ProcessFactory.Start(processConfig, false);
    var client = await server.WaitForClientConnect();
    return (client, processHandle);
  }

  public async Task RunDiscoverAsync(string testExecutablePath, string outFile, CancellationToken token)
  {
    var (client, handle) = await GetInitializedClient(testExecutablePath);
    await using var _ = client;
    var discovered = await client.DiscoverTestsAsync(token);
    var tests = discovered.Where(x => x != null && x.Node != null).Select(x => x.ToDiscoveredTest()).ToList();
    outFileWriterService.WriteDiscoveredTests(tests, outFile);
  }

  public async Task RunTestsAsync(string testExecutablePath, RunRequestNode[] filter, string outFile, CancellationToken token)
  {
    var (client, handle) = await GetInitializedClient(testExecutablePath);
    await using var _ = client;
    var runResults = await client.RunTestsAsync(filter, token);
    var results = runResults.Select(x => x.ToTestRunResult()).ToList();
    outFileWriterService.WriteTestRunResults(results, outFile);
  }
}