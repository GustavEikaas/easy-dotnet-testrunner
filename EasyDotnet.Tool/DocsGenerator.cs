using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using EasyDotnet.Utils;
using StreamJsonRpc;

namespace EasyDotnet;


public static class RpcDocGenerator
{
  public static string GenerateJsonDoc()
  {
    var allDocs = AssemblyScanner.GetControllerTypes()
        .Select(rpcType =>
        {
          var methods = rpcType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
              .Where(m => m.GetCustomAttribute<JsonRpcMethodAttribute>() is not null)
              .Select(m =>
              {
                var attr = m.GetCustomAttribute<JsonRpcMethodAttribute>();
                return new RpcMethodInfo
                {
                  Name = m.Name,
                  RpcPath = attr?.Name ?? m.Name,
                  Parameters = m.GetParameters()
                          .Select(p => new RpcParameter
                          {
                            Name = p.Name ?? "",
                            Type = GetFriendlyTypeName(p.ParameterType),
                            IsOptional = p.IsOptional
                          })
                          .ToList(),
                  ReturnType = GetFriendlyTypeName(m.ReturnType)
                };
              })
              .ToList();

          return new RpcApiDoc
          {
            ClassName = rpcType.Name,
            Methods = methods
          };
        })
        .Where(doc => doc.Methods.Count > 0)
        .ToList();

    return JsonSerializer.Serialize(allDocs, new JsonSerializerOptions { WriteIndented = true });
  }


  private static string GetFriendlyTypeName(Type type)
  {
    if (type.IsGenericType)
    {
      var typeDef = type.GetGenericTypeDefinition();
      var genericArgs = type.GetGenericArguments().Select(GetFriendlyTypeName);

      // Handle common wrappers
      if (typeDef == typeof(Nullable<>))
        return $"{genericArgs.First()}?";

      var baseName = typeDef.Name.Split('`')[0]; // Strip `1, `2, etc.
      return $"{baseName}<{string.Join(", ", genericArgs)}>";
    }

    return type.Name switch
    {
      "String" => "string",
      "Int32" => "int",
      "Boolean" => "bool",
      "Object" => "object",
      _ => type.Name
    };
  }

  private class RpcApiDoc
  {
    public string ClassName { get; set; } = "";
    public List<RpcMethodInfo> Methods { get; set; } = new();
  }

  private class RpcMethodInfo
  {
    public string Name { get; set; } = "";
    public string RpcPath { get; set; } = "";
    public List<RpcParameter> Parameters { get; set; } = new();
    public string ReturnType { get; set; } = "";
  }

  private class RpcParameter
  {
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public bool IsOptional { get; set; }
  }
}