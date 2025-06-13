using System.Collections.Generic;
using EasyDotnet.Msbuild.Models;
using Microsoft.Build.Framework;

namespace EasyDotnet.Msbuild;

public class InMemoryLogger : ILogger
{
  public List<BuildMessage> Messages { get; } = [];

  public LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Normal;
  public string? Parameters { get; set; }

  public void Initialize(IEventSource eventSource)
  {
    eventSource.ErrorRaised += (sender, args) =>
      Messages.Add(
        new BuildMessage(
          "error",
          args.File,
          args.LineNumber,
          args.ColumnNumber,
          args.Code,
          args?.Message
        )
      );
    eventSource.WarningRaised += (sender, args) =>
      Messages.Add(
        new BuildMessage(
          "warning",
          args.File,
          args.LineNumber,
          args.ColumnNumber,
          args.Code,
          args?.Message
        )
      );
  }

  public void Shutdown() { }
}