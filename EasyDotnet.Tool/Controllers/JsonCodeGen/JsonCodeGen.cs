using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyDotnet.Utils;
using Microsoft.CodeAnalysis.MSBuild;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.JsonCodeGen;

public class JsonCodeGen : BaseController
{
  [JsonRpcMethod("json-code-gen")]
  public static async Task<IEnumerable<object>> JsonToCode(string jsonData, string filePath)
  {
    var schema = JsonSchema.FromSampleJson(jsonData);

    var generator = new CSharpGenerator(schema, new CSharpGeneratorSettings
    {
      GenerateDataAnnotations = false,
      GenerateJsonMethods = false,
      JsonLibrary = CSharpJsonLibrary.SystemTextJson, // This should remove Newtonsoft attributes
      GenerateOptionalPropertiesAsNullable = false,
      GenerateNullableReferenceTypes = false,
      ClassStyle = CSharpClassStyle.Poco,
      GenerateDefaultValues = false,
      HandleReferences = false,
      RequiredPropertiesMustBeDefined = false
    });

    schema.AllowAdditionalProperties = false;

    var className = Path.GetFileNameWithoutExtension(filePath).Split(".").ElementAt(0)!;
    var code = generator.GenerateFile(className);

    var projectPath = FindCsprojFromFile(filePath);
    using var workspace = MSBuildWorkspace.Create();
    var project = await workspace.OpenProjectAsync(projectPath);

    var rootNamespace = project.DefaultNamespace;

    var relativePath = Path.GetDirectoryName(filePath)!
        .Replace(Path.GetDirectoryName(projectPath)!, "")
        .Trim(Path.DirectorySeparatorChar);
    var nsSuffix = relativePath.Replace(Path.DirectorySeparatorChar, '.');
    var fullNamespace = string.IsNullOrEmpty(nsSuffix) ? rootNamespace : $"{rootNamespace}.{nsSuffix}";

    var cleanClassOnly = NJsonClassExtractor.ExtractClassWithNamespace(code, fullNamespace!);

    File.WriteAllText(filePath, cleanClassOnly);
    return [];
  }

  private static string FindCsprojFromFile(string filePath)
  {
    var dir = Path.GetDirectoryName(filePath)
        ?? throw new ArgumentException("Invalid file path", nameof(filePath));

    return FindCsprojInDirectoryOrParents(dir)
        ?? throw new FileNotFoundException($"Failed to resolve csproj for file: {filePath}");
  }

  private static string? FindCsprojInDirectoryOrParents(string directory)
  {
    var csproj = Directory.GetFiles(directory, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
    if (csproj != null)
    {
      return csproj;
    }

    var parent = Directory.GetParent(directory);
    return parent != null
        ? FindCsprojInDirectoryOrParents(parent.FullName)
        : null;
  }
}