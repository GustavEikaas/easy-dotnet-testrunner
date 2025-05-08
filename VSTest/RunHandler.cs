using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using EasyDotnet.Types;

using Microsoft.TestPlatform.VsTestConsole.TranslationLayer;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace EasyDotnet.VSTest;

public static class RunHandler
{
  public static List<TestRunResult> RunTests(string vsTestPath, string dllPath, string filter)
  {
    var options = new TestPlatformOptions
    {
      CollectMetrics = false,
      SkipDefaultAdapters = false
    };

    var r = new VsTestConsoleWrapper(vsTestPath);
    var sessionHandler = new TestSessionHandler();
    var handler = new TestRunHandler();
    r.RunTests((List<TestCase>)[], null, options, sessionHandler.TestSessionInfo, handler);
    return handler.Results.Select(x => new TestRunResult() {Duration = (long?) x.Duration.TotalMilliseconds, StackTrace = x.ErrorStackTrace, ErrorMessage = x.ErrorMessage, Id = x.TestCase.Id.ToString(), Outcome = x.Outcome.ToString()}).ToList();
  }

  public class TestRunHandler() : ITestRunEventsHandler
  {
    public List<TestResult> Results = [];

    public void HandleLogMessage(TestMessageLevel level, string? message) { }

    public void HandleRawMessage(string rawMessage) { }

    public void HandleTestRunComplete(TestRunCompleteEventArgs testRunCompleteArgs, TestRunChangedEventArgs? lastChunkArgs, ICollection<AttachmentSet>? runContextAttachments, ICollection<string>? executorUris) { }
    public void HandleTestRunStatsChange(TestRunChangedEventArgs? testRunChangedArgs)
    {
      if (testRunChangedArgs?.NewTestResults is not null)
      {
        Results.AddRange(testRunChangedArgs.NewTestResults);
      }
    }

    public int LaunchProcessWithDebuggerAttached(TestProcessStartInfo testProcessStartInfo)
    {
      throw new NotImplementedException();
    }
  }
}