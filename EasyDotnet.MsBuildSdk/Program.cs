using Microsoft.Build.Locator;
using Microsoft.Build.Evaluation;
using StreamJsonRpc;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace BuildServer
{
  class Program
  {
    static async Task Main(string[] args)
    {
      // Register MSBuild
      MSBuildLocator.RegisterDefaults();

      // Set up JSON-RPC over stdin/stdout
      var rpc = new JsonRpc(Console.OpenStandardOutput(), Console.OpenStandardInput(), new BuildHandler());
      rpc.StartListening();

      Console.Error.WriteLine("BuildServer started. Listening for RPC...");
      await Task.Delay(-1); // Keep app alive
    }
  }

  public class BuildHandler
  {
    [JsonRpcMethod("build")]
    public async Task<string> BuildAsync(string projectPath)
    {
      try
      {
        var project = new Project(projectPath);
        var success = project.Build();
        return success ? "Build succeeded." : "Build failed.";
      }
      catch (Exception ex)
      {
        return $"Build error: {ex.Message}";
      }
    }

    public BuildResult RequestBuild(string targetPath, string configuration)
    {
      var properties = new Dictionary<string, string?> { { "Configuration", configuration } };

      using var pc = new ProjectCollection();
      var buildRequest = new BuildRequestData(targetPath, properties, null, ["Restore", "Build"], null);
      var logger = new InMemoryLogger();

      var parameters = new BuildParameters(pc) { Loggers = [logger] };

      var result = BuildManager.DefaultBuildManager.Build(parameters, buildRequest);

      return new BuildResult(Success: result.OverallResult == BuildResultCode.Success, result, logger.Messages);
    }

    public record BuildResult(bool Success, Microsoft.Build.Execution.BuildResult Result, List<BuildMessage> Messages);

    public sealed record BuildMessage(string Type, string FilePath, int LineNumber, int ColumnNumber, string Code, string? Message);

    public class InMemoryLogger : ILogger
    {
      public List<BuildMessage> Messages { get; } = [];

      public LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Normal;
      public string? Parameters { get; set; }

      public void Initialize(IEventSource eventSource)
      {
        eventSource.ErrorRaised += (sender, args) => Messages.Add(new BuildMessage("error", args.File, args.LineNumber, args.ColumnNumber, args.Code, args?.Message));
        eventSource.WarningRaised += (sender, args) => Messages.Add(new BuildMessage("warning", args.File, args.LineNumber, args.ColumnNumber, args.Code, args?.Message));
      }

      public void Shutdown() { }
    }
  }
}