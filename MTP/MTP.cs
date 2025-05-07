using System.IO;
using System.Threading.Tasks;

namespace EasyDotnet.MTP;

public static class MTPHandler
{
  public static async Task RunDiscoverAsync(DiscoverRequest request)
  {
    if (!File.Exists(request.TestExecutablePath))
    {
      throw new FileNotFoundException("Test executable not found.", request.TestExecutablePath);
    }
    var tests = await DiscoverHandler.Discover(request.TestExecutablePath);
    TestWriter.WriteDiscoveredTests(tests, request.OutFile);
  }

  public static Task RunTestsAsync(IRunRequest request)
  {
    throw new System.NotImplementedException();
  }
}