using EasyDotnet.Playground.RPC;

namespace EasyDotnet.Playground;

class Program
{
  private static readonly string TestExe = "/home/gustav/repo/TestPlatform.Playground/MTP.TUnit.Tests/bin/Debug/net9.0/MTP.TUnit.Tests";

  public static async Task<int> Main(string[] args)
  {
    await using var client = await Client.CreateAsync(TestExe);
    var discovered = await client.DiscoverTestsAsync();
    Console.WriteLine($"Discovered {discovered.Length} tests");
    var testResults = await client.RunTestsAsync([.. discovered.Select(x => x.Node)]);
    Console.WriteLine($"Ran {testResults.Length} tests");
    return 0;
  }

}


