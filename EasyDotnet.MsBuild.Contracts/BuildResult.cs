namespace EasyDotnet.MsBuild.Contracts;

public record BuildResult(bool Success, List<BuildMessage> Errors, List<BuildMessage> Warnings);