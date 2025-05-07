using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using EasyDotnet.MTP.ServerMode;
using EasyDotnet.Types;

using Microsoft.Testing.Platform.ServerMode.IntegrationTests.Messages.V100;

namespace EasyDotnet.MTP;

public static class DiscoverHandler
{
  public static async Task<List<DiscoveredTest>> Discover(string testExecutablePath)
  {
    using TestingPlatformClient client = await TestingPlatformClientFactory.StartAsServerAndConnectToTheClientAsync(testExecutablePath);
    await client.InitializeAsync();

    List<TestNodeUpdate> testNodeUpdates = [];
    ResponseListener discoveryResponse = await client.DiscoverTestsAsync(Guid.NewGuid(), node =>
    {
      testNodeUpdates.AddRange(node);
      return Task.CompletedTask;
    });
    await discoveryResponse.WaitCompletionAsync();

    return [.. testNodeUpdates.Where(x => x != null && x.Node != null).Select(x => x.ToDiscoveredTest())];
  }
}