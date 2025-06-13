using System.Collections.Generic;
using System.Linq;
using EasyDotnet.Services;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.Nuget;

public class NugetController(ClientService clientService, NugetService nugetService) : BaseController
{
  [JsonRpcMethod("nuget/list-sources")]
  public List<NugetSourceResponse> GetSources(string name)
  {
    clientService.ThrowIfNotInitialized();

    var sources = nugetService.GetSources();
    return [.. sources.Select(x => x.ToResponse())];
  }

}