using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyDotnet.Services;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.Nuget;

public class NugetController(ClientService clientService, NugetService nugetService, OutFileWriterService outFileWriterService) : BaseController
{
  [JsonRpcMethod("nuget/list-sources")]
  public List<NugetSourceResponse> GetSources()
  {
    clientService.ThrowIfNotInitialized();

    var sources = nugetService.GetSources();
    return [.. sources.Select(x => x.ToResponse())];
  }

  [JsonRpcMethod("nuget/search-packages")]
  public async Task<FileResultResponse> SearchPackages(string searchTerm, List<string>? sources = null)
  {
    clientService.ThrowIfNotInitialized();

    var packages = await nugetService.SearchAllSourcesByNameAsync(searchTerm, new CancellationToken(), take: 10, includePrerelease: false, sources);

    var list = packages
        .SelectMany(kvp => kvp.Value.Select(x => NugetPackageMetadata.From(x, kvp.Key)))
        .ToList();

    var outFile = Path.GetTempFileName();
    outFileWriterService.WriteNugetResults(list, outFile);

    return new FileResultResponse(outFile);
  }
}