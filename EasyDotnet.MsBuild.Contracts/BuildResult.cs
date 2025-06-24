namespace EasyDotnet.MsBuild.Contracts;

public sealed record BuildResult(bool Success, List<BuildMessage> Errors, List<BuildMessage> Warnings);