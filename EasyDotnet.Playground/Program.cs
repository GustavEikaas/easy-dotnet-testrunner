using EasyDotnet.Playground.RPC;

namespace EasyDotnet.Playground;

class Program
{
  private static readonly string TestExe = "/home/gustav/repo/TestPlatform.Playground/MTP.TUnit.Tests/bin/Debug/net9.0/MTP.TUnit.Tests";

  public static async Task<int> Main(string[] args)
  {
    var client = new Client(TestExe);
    await client.Initialize();
    var tests = await client.RunTestsAsync();
    Console.WriteLine($"Ran {tests.Length} tests");
    await client.Terminate();
    return 0;
  }

}


