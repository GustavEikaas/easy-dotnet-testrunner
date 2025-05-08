using EasyDotnet.Types;

using Microsoft.Testing.Platform.ServerMode.IntegrationTests.Messages.V100;

namespace EasyDotnet.MTP;

public static class TestNodeExtensions
{
  public static DiscoveredTest ToDiscoveredTest(this TestNodeUpdate test)
  {
    //TODO: I need to document this somewhere
    var name = string.IsNullOrEmpty(test.Node.TestNamespace) ? test.Node.DisplayName : $"{test.Node.TestNamespace}.{test.Node.DisplayName}";
    return new()
    {
      Id = test.Node.Uid,
      FilePath = test.Node?.FilePath?.Replace("\\", "/"),
      LineNumber = test.Node?.LineStart,
      Namespace = test.Node.TestNamespace,
      DisplayName = test.Node.DisplayName,
      Name = name
    };
  }

  public static TestRunResult ToTestRunResult(this TestNodeUpdate test) => new()
  {
    Id = test.Node.Uid,
    Outcome = test.Node.ExecutionState,
    Duration = (long?)test.Node.Duration,
    ErrorMessage = test.Node.Message,
    StackTrace = test.Node.StackTrace
  };
}