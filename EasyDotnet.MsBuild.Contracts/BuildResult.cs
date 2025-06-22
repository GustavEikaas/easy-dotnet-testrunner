namespace EasyDotnet.MsBuild.Contracts;

public record BuildResult(bool Success, List<BuildMessage> Messages);