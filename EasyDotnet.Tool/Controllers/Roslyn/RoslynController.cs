using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyDotnet.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.Roslyn;

public class RoslynController(RoslynService roslynService, NotificationService notificationService) : BaseController
{

  [JsonRpcMethod("roslyn/bootstrap-file")]
  public async Task<BootstrapFileResultResponse> BootstrapFile(string filePath, Kind kind, bool preferFileScopedNamespace)
  {
    var success = await roslynService.BootstrapFile(filePath, kind, preferFileScopedNamespace, new CancellationToken());
    return new(success);
  }

  public class SymbolResult
  {
    public string Name { get; set; }
    public string Kind { get; set; }
    public string FilePath { get; set; }
    public int Line { get; set; }
  }

  [JsonRpcMethod("roslyn/symbol-search")]
  public async Task<List<SymbolResult>> SymbolSearch(string searchTerm, string[] projectPaths, string[] symbolTypes)
  {
    // Acrivity.Current.Id
    var results = new List<SymbolResult>();
    var notificationTasks = new List<Task>();

    foreach (var projectPath in projectPaths)
    {
      using var workspace = MSBuildWorkspace.Create();
      var project = await workspace.OpenProjectAsync(projectPath);
      var compilation = await project.GetCompilationAsync();
      if (compilation == null) continue;

      var treeResults = await Task.WhenAll(compilation.SyntaxTrees.Select(async tree =>
      {
        var semanticModel = compilation.GetSemanticModel(tree);
        var root = await tree.GetRootAsync();
        return root.DescendantNodes()
              .OfType<BaseTypeDeclarationSyntax>()
              .Select(node => semanticModel.GetDeclaredSymbol(node))
              .Where(symbol =>
                  symbol != null &&
                  MatchesTypeFilter(symbol, symbolTypes) &&
                  symbol.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                  )
              .Select(symbol => new SymbolResult
              {
                Name = symbol!.Name,
                Kind = GetSymbolCategory(symbol!) ?? symbol.Kind.ToString(),
                FilePath = symbol.Locations.FirstOrDefault()?.SourceTree?.FilePath ?? "unknown",
                Line = symbol.Locations.FirstOrDefault()?.GetLineSpan().StartLinePosition.Line ?? -1
              });
      }));

      // Collect all notification tasks without awaiting
      var projectNotificationTasks = treeResults
        .SelectMany(x => x)
        .Select(item => notificationService.StreamMessage(item));

      notificationTasks.AddRange(projectNotificationTasks);
      results.AddRange(treeResults.SelectMany(x => x));
    }

    // Await all notification tasks at the very end
    await Task.WhenAll(notificationTasks);

    return results;
  }


  private string? GetSymbolCategory(ISymbol symbol) => symbol switch
  {
    INamedTypeSymbol namedType => namedType.TypeKind switch
    {
      TypeKind.Class => namedType.IsRecord ? "record" : "class",
      TypeKind.Struct => namedType.IsRecord ? "recordstruct" : "struct",
      TypeKind.Interface => "interface",
      TypeKind.Enum => "enum",
      TypeKind.Delegate => "delegate",
      _ => null
    },
    IMethodSymbol => "method",
    IPropertySymbol => "property",
    IFieldSymbol => "field",
    ILocalSymbol => "local",
    IParameterSymbol => "parameter",
    IEventSymbol => "event",
    INamespaceSymbol => "namespace",
    _ => null
  };

  private bool MatchesTypeFilter(ISymbol symbol, string[] types)
  {
    var category = GetSymbolCategory(symbol);
    return category != null &&
           types.Any(t => t.Equals(category, StringComparison.OrdinalIgnoreCase));
  }
}