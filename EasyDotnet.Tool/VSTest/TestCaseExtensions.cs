using EasyDotnet.Types;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace EasyDotnet.VSTest;

public static class TestCaseExtensions
{
  public static DiscoveredTest ToDiscoveredTest(this TestCase x){
    var name = x.DisplayName.Contains('.') ? x.DisplayName : x.FullyQualifiedName;
    return new()
     {
       Id = x.Id.ToString(),
       Namespace = x.FullyQualifiedName,
       Name = name,
       FilePath = x.CodeFilePath?.Replace("\\", "/"),
       LineNumber = x.LineNumber,
       DisplayName = x.DisplayName
     };
  }
     

  public static TestRunResult ToTestRunResult(this TestResult x){

    return new()
     {
        Duration = (long?) x.Duration.TotalMilliseconds,
        StackTrace = x.ErrorStackTrace,
        ErrorMessage = x.ErrorMessage,
        Id = x.TestCase.Id.ToString(),
        Outcome = GetTestOutcome(x.Outcome)
     };
  }

  public static string GetTestOutcome(TestOutcome outcome){
    return outcome switch
    {
      TestOutcome.None => "none",
      TestOutcome.Passed => "passed",
      TestOutcome.Failed => "failed",
      TestOutcome.Skipped => "skipped",
      TestOutcome.NotFound => "not found",
      _ => "",
    };
  }
}