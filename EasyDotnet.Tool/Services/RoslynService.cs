using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyDotnet.Controllers.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace EasyDotnet.Services;

public class RoslynService
{

  public async Task<bool> BootstrapFile(string filePath, Kind kind, bool preferFileScopedNamespace, CancellationToken cancellationToken)
  {
    var projectPath = FindCsprojFromFile(filePath);
    using var workspace = MSBuildWorkspace.Create();
    var project = await workspace.OpenProjectAsync(projectPath, cancellationToken: cancellationToken);

    var rootNamespace = project.DefaultNamespace;
    if (string.IsNullOrEmpty(rootNamespace))
    {
      throw new Exception("root namespace cannot be null");
    }

    var parseOptions = project.ParseOptions as CSharpParseOptions;
    var langVersion = parseOptions?.LanguageVersion ?? LanguageVersion.CSharp9;

    var supportsFileScoped = langVersion >= LanguageVersion.CSharp10;
    var useFileScopedNs = preferFileScopedNamespace && supportsFileScoped;

    var relativePath = Path.GetDirectoryName(filePath)!
        .Replace(Path.GetDirectoryName(projectPath)!, "")
        .Trim(Path.DirectorySeparatorChar);
    var nsSuffix = relativePath.Replace(Path.DirectorySeparatorChar, '.');
    var fullNamespace = string.IsNullOrEmpty(nsSuffix) ? rootNamespace : $"{rootNamespace}.{nsSuffix}";

    var className = Path.GetFileNameWithoutExtension(filePath).Split(".").ElementAt(0)!;

    var typeDecl = CreateTypeDeclaration(kind, className, useFileScopedNs);

    MemberDeclarationSyntax nsDeclaration = useFileScopedNs
        ? SyntaxFactory.FileScopedNamespaceDeclaration(SyntaxFactory.ParseName(fullNamespace))
            .AddMembers(typeDecl)
        : SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(fullNamespace))
            .AddMembers(typeDecl);

    var unit = SyntaxFactory.CompilationUnit()
        .AddMembers(nsDeclaration)
        .NormalizeWhitespace();

    if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
    {
      return false;
    }

    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
    File.WriteAllText(filePath, unit.ToFullString());

    return true;
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

  private static MemberDeclarationSyntax CreateTypeDeclaration(Kind kind, string className, bool useFileScopedNs)
  {
    var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

    return kind switch
    {
      Kind.Class => SyntaxFactory.ClassDeclaration(className)
          .WithModifiers(modifiers)
          .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
          .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken))
          .WithLeadingTrivia(useFileScopedNs
              ? SyntaxFactory.TriviaList(SyntaxFactory.CarriageReturnLineFeed, SyntaxFactory.CarriageReturnLineFeed)
              : SyntaxFactory.TriviaList(SyntaxFactory.CarriageReturnLineFeed)),

      Kind.Interface => SyntaxFactory.InterfaceDeclaration(className)
          .WithModifiers(modifiers)
          .WithLeadingTrivia(useFileScopedNs
              ? SyntaxFactory.TriviaList(SyntaxFactory.CarriageReturnLineFeed, SyntaxFactory.CarriageReturnLineFeed)
              : SyntaxFactory.TriviaList(SyntaxFactory.CarriageReturnLineFeed)),

      Kind.Record => SyntaxFactory.RecordDeclaration(
              SyntaxFactory.Token(SyntaxKind.RecordKeyword),
              SyntaxFactory.Identifier(className))
          .WithModifiers(modifiers)
          .WithParameterList(SyntaxFactory.ParameterList())
          .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
          .WithLeadingTrivia(useFileScopedNs
              ? SyntaxFactory.TriviaList(SyntaxFactory.CarriageReturnLineFeed, SyntaxFactory.CarriageReturnLineFeed)
              : SyntaxFactory.TriviaList(SyntaxFactory.CarriageReturnLineFeed)),

      _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
  }
}