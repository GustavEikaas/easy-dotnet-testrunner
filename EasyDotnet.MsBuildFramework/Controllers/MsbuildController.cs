using System;
using System.Collections.Generic;
using EasyDotnet.MsBuild.Contracts;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using StreamJsonRpc;

namespace EasyDotnet.MsBuildFramework.Controllers
{
  public class MsbuildController
  {

    [JsonRpcMethod("msbuild/build")]
    public MsBuild.Contracts.BuildResult RequestBuild(string targetPath, string configuration)
    {

      var properties = new Dictionary<string, string>
    {
      { "Configuration", "Debug" },
      { "Platform", "AnyCPU" },
      { "BuildingInsideVisualStudio", "true" },
      { "DesignTimeBuild", "false" },
    };


      using (var pc = new ProjectCollection())
      {
        var buildRequest = new BuildRequestData(
            targetPath,
            properties,
            null,
            new[] { "Build" },
            null
        );

        var logger = new InMemoryLogger();

        var parameters = new BuildParameters(pc)
        {
          Loggers = new List<ILogger> { logger }
        };

        var result = BuildManager.DefaultBuildManager.Build(parameters, buildRequest);

        return new MsBuild.Contracts.BuildResult(
            result.OverallResult == BuildResultCode.Success,
            logger.Errors,
            logger.Warnings
        );
      }
    }
  }

  public class InMemoryLogger : ILogger
  {
    public List<BuildMessage> Errors { get; } = new List<BuildMessage>();
    public List<BuildMessage> Warnings { get; } = new List<BuildMessage>();

    public LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Normal;
    public string Parameters { get; set; }

    public void Initialize(IEventSource eventSource)
    {
      eventSource.ErrorRaised += (sender, args) => Errors.Add(new BuildMessage("error", args.File, args.LineNumber, args.ColumnNumber, args.Code, args?.Message));
      eventSource.WarningRaised += (sender, args) => Warnings.Add(new BuildMessage("warning", args.File, args.LineNumber, args.ColumnNumber, args.Code, args?.Message));
    }

    public void Shutdown() { }
  }
}