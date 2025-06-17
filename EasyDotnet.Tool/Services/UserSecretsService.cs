using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;

namespace EasyDotnet.Services;

public sealed record ProjectUserSecret(string Id, string FilePath);

public class UserSecretsService
{

  private readonly string _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "UserSecrets");

  public ProjectUserSecret AddUserSecretsId(string projectPath)
  {
    if (!File.Exists(projectPath))
    {
      throw new FileNotFoundException("Project file not found", projectPath);
    }

    var projectCollection = new ProjectCollection();
    var project = projectCollection.LoadProject(projectPath);

    var currentSecretsId = project.GetPropertyValue("UserSecretsId");
    if (!string.IsNullOrEmpty(currentSecretsId))
    {
      var path = GetSecretsPath(currentSecretsId);
      return new(currentSecretsId, path);
    }

    var newSecretsId = Guid.NewGuid().ToString();

    var propertyGroup = project.Xml.PropertyGroups
        .FirstOrDefault(pg => pg.ConditionLocation is null)
        ?? project.Xml.AddPropertyGroup();

    propertyGroup.AddProperty("UserSecretsId", newSecretsId);

    project.Save();

    EnsureSecretsDirectory(newSecretsId);
    var secretsFilePath = GetSecretsPath(newSecretsId);

    if (!File.Exists(secretsFilePath))
    {
      File.WriteAllText(secretsFilePath, "{ }");
    }
    return new(newSecretsId, secretsFilePath);
  }

  private void EnsureSecretsDirectory(string id)
  {
    var secretsDir = Path.Combine(_basePath, id);
    if (!Directory.Exists(secretsDir))
    {
      Directory.CreateDirectory(secretsDir);
    }
  }

  private string GetSecretsPath(string id)
  {
    var secretsDir = Path.Combine(_basePath, id);
    var secretsFilePath = Path.Combine(secretsDir, "secrets.json");
    return secretsFilePath;
  }
}