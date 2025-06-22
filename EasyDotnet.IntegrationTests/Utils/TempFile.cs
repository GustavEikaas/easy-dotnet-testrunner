namespace EasyDotnet.IntegrationTests.Utils;

public sealed class TempFile : IDisposable
{
  public readonly string Path = System.IO.Path.GetTempFileName();

  public void Dispose() => File.Delete(Path);

  public override string ToString() => Path;
}