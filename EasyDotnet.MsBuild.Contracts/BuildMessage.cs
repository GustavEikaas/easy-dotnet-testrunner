namespace EasyDotnet.MsBuild.Contracts
{
  public sealed class BuildMessage
  {
    public string Type { get; }
    public string FilePath { get; }
    public int LineNumber { get; }
    public int ColumnNumber { get; }
    public string Code { get; }
    public string Message { get; }

    public BuildMessage(string type, string filePath, int lineNumber, int columnNumber, string code, string message)
    {
      Type = type;
      FilePath = filePath;
      LineNumber = lineNumber;
      ColumnNumber = columnNumber;
      Code = code;
      Message = message;
    }
  }
}