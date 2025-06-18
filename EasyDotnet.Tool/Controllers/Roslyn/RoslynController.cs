using System.Threading;
using System.Threading.Tasks;
using EasyDotnet.Services;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.Roslyn;

public class RoslynController(RoslynService roslynService) : BaseController
{

  // [JsonRpcMethod("roslyn/sln-open")]
  // public async Task SymbolSearch(string solutionPath)
  // {
  //   var workspace = MSBuildWorkspace.Create();
  //   var solution = await workspace.OpenSolutionAsync(solutionPath);
  //
  //   foreach (var project in solution.Projects)
  //   {
  //     var compilation = await project.GetCompilationAsync();
  //     var symbols = await SymbolFinder.FindSourceDeclarationsAsync(
  //         project, name => true); // You can filter names here
  //
  //     var classSymbols = symbols
  //         .OfType<INamedTypeSymbol>()
  //         .Where(s => s.TypeKind == TypeKind.Class);
  //
  //     foreach (var classSymbol in classSymbols)
  //     {
  //       Console.WriteLine($"Class: {classSymbol.Name} in {classSymbol.ContainingNamespace}");
  //     }
  //   }
  // }

  [JsonRpcMethod("roslyn/bootstrap-file")]
  public async Task<BootstrapFileResultResponse> BootstrapFile(string filePath, Kind kind, bool fileScopedNsPreference)
  {
    var success = await roslynService.BootstrapFile(filePath, kind, fileScopedNsPreference, new CancellationToken());
    return new(success);

  }
}