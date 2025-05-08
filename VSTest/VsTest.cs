using System.IO;
using System.Threading.Tasks;

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

  public static Task RunTestsAsync(RunRequest request)
  {
    throw new System.NotImplementedException();
  }

}