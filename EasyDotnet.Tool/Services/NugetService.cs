using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace EasyDotnet.Services;

public class NugetService
{

  public List<PackageSource> GetSources()
  {
    var settings = Settings.LoadDefaultSettings(root: null);
    var sourceProvider = new PackageSourceProvider(settings);
    var sources = sourceProvider.LoadPackageSources();
    return [.. sources];
  }

  public async Task<Dictionary<string, IEnumerable<IPackageSearchMetadata>>> SearchAllSourcesByNameAsync(
        string searchTerm,
        CancellationToken cancellationToken,
        int take = 10,
        bool includePrerelease = false,
        List<string>? sourceNames = null)
  {
    var provider = Repository.Provider.GetCoreV3();

    var sourceProvider = new PackageSourceProvider(Settings.LoadDefaultSettings(null));
    var allSources = sourceProvider.LoadPackageSources().Where(s => s.IsEnabled);

    var selectedSources = sourceNames == null ? allSources : allSources.Where(s => sourceNames.Contains(s.Name, StringComparer.OrdinalIgnoreCase));

    var taskMap = selectedSources.ToDictionary(
        source => source.Name,
        async source =>
        {
          try
          {
            var repo = new SourceRepository(source, provider);
            var search = await repo.GetResourceAsync<PackageSearchResource>();

            return await search.SearchAsync(
                    searchTerm,
                    new SearchFilter(includePrerelease),
                    skip: 0,
                    take: take,
                    log: NullLogger.Instance,
                    cancellationToken: cancellationToken);
          }
          catch
          {
            return [];
          }
        });

    await Task.WhenAll(taskMap.Values);

    return taskMap.ToDictionary(
        kvp => kvp.Key,
        kvp => kvp.Value.Result);
  }

  public async Task<bool> PushPackageAsync(List<string> packages, string sourceUrl, string apiKey)
  {
    var notFound = packages.FirstOrDefault(x => !File.Exists(x));
    if (notFound is not null)
    {
      throw new FileNotFoundException("Package not found", notFound);
    }

    var packageUpdateResource = await GetPackageUpdateResourceAsync(sourceUrl);

    await packageUpdateResource.Push(
        packages,
        symbolSource: null,
        timeoutInSecond: 300,
        disableBuffering: false,
        getApiKey: _ => apiKey,
        getSymbolApiKey: null,
        noServiceEndpoint: false,
        skipDuplicate: false,
        symbolPackageUpdateResource: null,
        log: NullLogger.Instance
    );

    return true;
  }

  private async Task<PackageUpdateResource> GetPackageUpdateResourceAsync(string sourceUrl)
  {
    var packageSource = new PackageSource(sourceUrl);
    var sourceRepository = Repository.Factory.GetCoreV3(packageSource);
    return await sourceRepository.GetResourceAsync<PackageUpdateResource>();
  }
}