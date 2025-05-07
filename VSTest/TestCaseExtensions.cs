using EasyDotnet.Types;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace EasyDotnet.VSTest;

public static class TestCaseExtensions
{
  public static DiscoveredTest Map(this TestCase x) =>
     new()
     {
       Id = x.Id.ToString(),
       Namespace = x.FullyQualifiedName,
       Name = x.DisplayName,
       //TODO: replace all \ with /
       FilePath = x.CodeFilePath.Replace("\\", "/"),
       LineNumber = x.LineNumber
     };


}