using System.Collections.Generic;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace EasyDotnet.Services;

public class MsBuildService
{

  public BuildResult RequestBuild(string targetPath, string configuration)
  {
    var properties = new Dictionary<string, string?> { { "Configuration", configuration } };

    var pc = new ProjectCollection(properties);
    var buildRequest = new BuildRequestData(targetPath, properties, null, ["Restore", "Build"], null);
    var logger = new InMemoryLogger();

    var parameters = new BuildParameters(pc) { Loggers = [logger] };

    var result = BuildManager.DefaultBuildManager.Build(parameters, buildRequest);

    return new BuildResult(result, logger.Messages);
  }
}

public record BuildResult(Microsoft.Build.Execution.BuildResult Result, List<BuildMessage> Messages);


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