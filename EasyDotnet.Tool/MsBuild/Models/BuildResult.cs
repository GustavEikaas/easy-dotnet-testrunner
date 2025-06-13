using System.Collections.Generic;

namespace EasyDotnet.Msbuild.Models;

public record BuildResult(Microsoft.Build.Execution.BuildResult Result, List<BuildMessage> Messages);
