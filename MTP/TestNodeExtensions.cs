using EasyDotnet.Types;

using Microsoft.Testing.Platform.ServerMode.IntegrationTests.Messages.V100;

namespace EasyDotnet.MTP;

public static class TestNodeExtensions
{
  public static DiscoveredTest ToDiscoveredTest(this TestNodeUpdate test) => new()
  {
    Id = test.Node.Uid,
    FilePath = test.Node.FilePath.Replace("\\", "/"),
    LineNumber = test.Node.LineStart,
    Namespace = test.Node.TestNamespace,
    Name = test.Node.DisplayName
  };

}