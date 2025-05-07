using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using EasyDotnet.MTP.ServerMode;
using EasyDotnet.Types;

using Microsoft.Testing.Platform.ServerMode.IntegrationTests.Messages.V100;

namespace EasyDotnet.MTP;

public static class RunHandler
{
  public static async Task<List<TestRunResult>> RunTests(RunRequest request)
  {
    using TestingPlatformClient client = await TestingPlatformClientFactory.StartAsServerAndConnectToTheClientAsync(request.TestExecutablePath);
    await client.InitializeAsync();

    List<TestNodeUpdate> runResults = [];
    ResponseListener runRequest = await client.RunTestsAsync(Guid.NewGuid(), [.. request.Filter], node =>
    {
      runResults.AddRange(node);
      return Task.CompletedTask;
    });
    await runRequest.WaitCompletionAsync();
    return [.. runResults.Where(x => x.Node.ExecutionState != "in-progress").Select(x => x.ToTestRunResult())];
  }

}