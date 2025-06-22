
using System.IO;

namespace EasyDotnet.Utils;

public static class HostDirectoryUtil
{
  public static string HostDirectory { get; set; } = Directory.GetCurrentDirectory();
}