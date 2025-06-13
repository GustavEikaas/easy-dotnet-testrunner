using System.Collections.Generic;
using NuGet.Configuration;

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
}