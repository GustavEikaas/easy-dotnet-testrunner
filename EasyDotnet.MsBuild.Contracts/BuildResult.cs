namespace EasyDotnet.MsBuild.Contracts;

public sealed record BuildResult(bool Success, List<string> Errors, List<string> Warnings);