using System;
using System.Collections.Generic;

using EasyDotnet.MsBuild.Models;

using Microsoft.Build.Evaluation;

namespace EasyDotnet.MsBuild;

public static class MsBuild
{

  public static DotnetProjectProperties QueryProject(string targetPath, string configuration){

      var properties = new Dictionary<string, string?>
      {
          { "Configuration", configuration }
      };

      var pc = new ProjectCollection(properties);
      var project = pc.LoadProject(targetPath);
      project.ReevaluateIfNecessary();

      return new DotnetProjectProperties(
          OutputPath: project.GetPropertyValue("OutputPath"),
          OutputType: project.GetPropertyValue("OutputType"),
          TargetExt: project.GetPropertyValue("TargetExt"),
          AssemblyName: project.GetPropertyValue("AssemblyName"),
          TargetFramework: project.GetPropertyValue("TargetFramework"),
          TargetFrameworks: project.GetPropertyValue("TargetFrameworks"),
          IsTestProject: GetBoolProperty(project, "IsTestProject"),
          UserSecretsId: project.GetPropertyValue("UserSecretsId"),
          TestingPlatformDotnetTestSupport: project.GetPropertyValue("TestingPlatformDotnetTestSupport"),
          TargetPath: project.GetPropertyValue("TargetPath"),
          GeneratePackageOnBuild: GetBoolProperty(project, "GeneratePackageOnBuild"),
          IsPackable: GetBoolProperty(project, "IsPackable"),
          PackageId: project.GetPropertyValue("PackageId"),
          Version: project.GetPropertyValue("Version"),
          PackageOutputPath: project.GetPropertyValue("PackageOutputPath")
      );
  }

  private static bool GetBoolProperty(Project project, string name) =>
    string.Equals(project.GetPropertyValue(name), "true", StringComparison.OrdinalIgnoreCase);
}
