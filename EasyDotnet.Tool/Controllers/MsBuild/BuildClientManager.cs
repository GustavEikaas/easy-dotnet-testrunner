using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace EasyDotnet.Controllers.MsBuild;

public enum BuildClientType
{
  Sdk,
  Framework
}

public interface IBuildClientManager
{
  Task<BuildClient> GetOrStartClientAsync(BuildClientType type);
  void StopAll();
}

public class BuildClientManager : IBuildClientManager, IDisposable
{
  private const int MaxPipeNameLength = 104;
  private readonly string _sdk_Pipe = GeneratePipeName(BuildClientType.Sdk);
  private readonly string _framework_Pipe = GeneratePipeName(BuildClientType.Framework);

  private readonly ConcurrentDictionary<string, BuildClient> _buildClientCache = new();


  public async Task<BuildClient> GetOrStartClientAsync(BuildClientType type)
  {
    var client = _buildClientCache.AddOrUpdate(
    type == BuildClientType.Sdk ? _sdk_Pipe : _framework_Pipe,
    key => new BuildClient(key),
    (key, existingClient) =>
      existingClient ?? new BuildClient(key));

    await client.ConnectAsync(ensureServerStarted: true);
    return client;
  }


  private static string GeneratePipeName(BuildClientType type)
  {
    var pipePrefix = "EasyDotnet_MSBuild_";
    var name = $"{pipePrefix}{type}_{Guid.NewGuid():N}";
    return name[..Math.Min(name.Length, MaxPipeNameLength)];
  }

  public void StopAll()
  {
    foreach (var dict in _buildClientCache.Values)
    {
      dict.StopServer();
    }
    _buildClientCache.Clear();
  }

  public void Dispose() => StopAll();
}