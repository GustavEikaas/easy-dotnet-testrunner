namespace EasyDotnet.Msbuild.Models;

public sealed record BuildMessage(
string Type,
string FilePath,
int LineNumber,
int ColumnNumber,
string Code,
string? Message
);
