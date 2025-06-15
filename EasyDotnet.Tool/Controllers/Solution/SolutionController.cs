using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Construction;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.Solution;

public class SolutionController() : BaseController
{
  [JsonRpcMethod("solution/list-projects")]
  public List<SolutionFileProjectResponse> ListProjects(string solutionFilePath)
  {
    var solution = SolutionFile.Parse(solutionFilePath);
    var projects = solution.ProjectsInOrder
        .Where(p => p.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat)
        .Select(x => x.ToResponse())
        .ToList();

    return projects;
  }

}