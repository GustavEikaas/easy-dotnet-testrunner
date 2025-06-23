using System;
using System.Linq;
using EasyDotnet.VSTest;

namespace EasyDotnet.Services;

public class VsTestService(OutFileWriterService outFileWriter)
{
  public void RunDiscover(string vsTestPath, DiscoverProjectRequest[] projects)
  {
    var dllPaths = projects.Select(x => x.DllPath).ToArray();
    var discoveredTests = DiscoverHandler.Discover(vsTestPath, dllPaths);

    projects
          .Join(
              discoveredTests,
              proj => proj.DllPath,
              test => test.Key.Replace("\\", "/"),
              (proj, test) => new { proj.OutFile, Tests = test.Value }
          )
          .ToList().ForEach(x => outFileWriter.WriteDiscoveredTests(x.Tests, x.OutFile));
  }

  public void RunTests(
    string vsTestPath,
    string dllPath,
    Guid[] testIds,
    string outFile
)
  {
    var testResults = RunHandler.RunTests(vsTestPath, dllPath, testIds);
    outFileWriter.WriteTestRunResults(testResults, outFile);
  }

}

public sealed record DiscoverProjectRequest(string DllPath, string OutFile);