using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;
using EasyDotnet.MsBuild.Contracts;
using EasyDotnet.Utils;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json.Serialization;
using StreamJsonRpc;

namespace EasyDotnet.Services;

public enum BuildClientType
{
  Sdk,
  Framework
}

public interface IMsBuildHostManager
{
  Task<MsBuildHost> GetOrStartClientAsync(BuildClientType type);
  void StopAll();
}

public class MsBuildHostManager : IMsBuildHostManager, IDisposable
{
  private const int MaxPipeNameLength = 104;
  private readonly string _sdk_Pipe = GeneratePipeName(BuildClientType.Sdk);
  private readonly string _framework_Pipe = GeneratePipeName(BuildClientType.Framework);

  private readonly ConcurrentDictionary<string, MsBuildHost> _buildClientCache = new();


  public async Task<MsBuildHost> GetOrStartClientAsync(BuildClientType type)
  {
    var client = _buildClientCache.AddOrUpdate(
    type == BuildClientType.Sdk ? _sdk_Pipe : _framework_Pipe,
    key => new MsBuildHost(key, type),
    (key, existingClient) =>
      existingClient ?? new MsBuildHost(key, type));

    await client.ConnectAsync(ensureServerStarted: true);
    return client;
  }


  private static string GeneratePipeName(BuildClientType type)
  {
    var pipePrefix = "EasyDotnet_MSBuild_";
    var name = $"{pipePrefix}{type}_{Guid.NewGuid():N}";
    return name[..Math.Min(name.Length, MaxPipeNameLength)];
  }

  public void StopAll()
  {
    _buildClientCache.Values.ToList().ForEach(x => x.StopServer());
    _buildClientCache.Clear();
  }

  public void Dispose() => StopAll();
}

public class MsBuildHost(string pipeName, BuildClientType type)
{
  private JsonRpc? _rpc;
  private Process? _serverProcess;
  private Task? _connectTask;
  private readonly object _connectLock = new();
  private readonly string _pipeName = pipeName;

  public Task ConnectAsync(bool ensureServerStarted = true)
  {
    lock (_connectLock)
    {
      _connectTask ??= ConnectInternalAsync(ensureServerStarted);
      return _connectTask;
    }
  }

  private async Task ConnectInternalAsync(bool ensureServerStarted)
  {
    if (ensureServerStarted)
    {
      _serverProcess = BuildServerStarter.StartBuildServer(_pipeName, type);
      await Task.Delay(1000);
    }

    var stream = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
    await stream.ConnectAsync();

    var jsonMessageFormatter = new JsonMessageFormatter
    {
      JsonSerializer = { ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() } }
    };

    var handler = new HeaderDelimitedMessageHandler(stream, stream, jsonMessageFormatter);
    _rpc = new JsonRpc(handler);
    _rpc.StartListening();
  }

  public async Task<BuildResult> BuildAsync(string targetPath, string configuration)
  {
    if (_rpc == null)
      throw new InvalidOperationException("BuildClient not connected.");

    var request = new { TargetPath = targetPath, Configuration = configuration };
    return await _rpc.InvokeWithParameterObjectAsync<BuildResult>("msbuild/build", request);
  }

  public void StopServer()
  {
    if (_serverProcess != null && !_serverProcess.HasExited)
    {
      _serverProcess.Kill(true);
      _serverProcess.Dispose();
    }
  }
}

public static class BuildServerStarter
{
  public static Process StartBuildServer(string pipeName, BuildClientType type)
  {
    var exePath = GetExecutablePathForClientType(type);

    if (!File.Exists(exePath))
    {
      throw new FileNotFoundException("Build server executable not found.", exePath);
    }
    Console.WriteLine(exePath);


    ProcessStartInfo startInfo;

    if (type == BuildClientType.Framework)
    {
      // Run the .NET Framework exe directly
      startInfo = new ProcessStartInfo
      {
        FileName = exePath,
        Arguments = pipeName,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
      };
    }
    else
    {
      // Run .NET Core/.NET SDK exe via dotnet command
      startInfo = new ProcessStartInfo
      {
        FileName = "dotnet",
        Arguments = $"\"{exePath}\" {pipeName}",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
      };
    }
    var process = new Process { StartInfo = startInfo };

    void ProcessExitHandler(object? sender, EventArgs e)
    {
      if (!process.HasExited)
      {
        process.Kill(true);
      }
      process.Dispose();
    }
    AppDomain.CurrentDomain.ProcessExit += ProcessExitHandler;

    process.WaitForExitAsync().ContinueWith(x => AppDomain.CurrentDomain.ProcessExit -= ProcessExitHandler);
    process.OutputDataReceived += (sender, e) =>
    {
      if (!string.IsNullOrEmpty(e.Data))
        Console.WriteLine($"[STDOUT] {e.Data}");
    };

    process.ErrorDataReceived += (sender, e) =>
    {
      if (!string.IsNullOrEmpty(e.Data))
        Console.WriteLine($"[STDERR] {e.Data}");
    };
    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    Console.WriteLine($"Started BuildServer from: {exePath}");

    return process;
  }

  private static string GetExecutablePathForClientType(BuildClientType type)
  {
    var dir = HostDirectoryUtil.HostDirectory;
    var exeHost = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

    return type switch
    {
#if DEBUG
      BuildClientType.Sdk => Path.Combine(dir, "EasyDotnet.MsBuildSdk", "bin", "Debug", "net8.0", "EasyDotnet.MsBuildSdk.dll"),
#else
      BuildClientType.Sdk => Path.Combine(exeHost,  "EasyDotnet.MsBuildSdk", "EasyDotnet.MsBuildSdk.dll"),
#endif
#if DEBUG
      BuildClientType.Framework => Path.Combine(dir, "EasyDotnet.MsBuildFramework", "bin", "Debug", "net472", "EasyDotnet.MsBuildFramework.exe"),
#else
      BuildClientType.Framework => Path.Combine(exeHost,  "MsBuildFramework", "EasyDotnet.MsBuildFramework.exe"),
#endif
      _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
  }
}