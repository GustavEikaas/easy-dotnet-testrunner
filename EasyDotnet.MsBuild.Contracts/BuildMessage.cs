namespace EasyDotnet.MsBuild.Contracts;

public sealed record BuildMessage(string Type, string FilePath, int LineNumber, int ColumnNumber, string Code, string? Message);