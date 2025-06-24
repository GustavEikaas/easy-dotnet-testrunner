using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using EasyDotnet.MsBuild.Contracts;
using EasyDotnet.Services;
using EasyDotnet.Utils;
using Newtonsoft.Json.Serialization;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.MsBuild;

public class MsBuildController(ClientService clientService, MsBuildService msBuild, OutFileWriterService outFileWriterService, IBuildClientManager manager) : BaseController
{
  [JsonRpcMethod("msbuild/build")]
  public async Task<BuildResultResponse> Build(BuildRequest request)
  {
    Console.WriteLine("Started: " + request.TargetPath);
    var x = await manager.GetOrStartClientAsync(BuildClientType.Sdk);
    var result = await x.BuildAsync(request.TargetPath, request.ConfigurationOrDefault);

    Console.WriteLine($"Finished: {request.TargetPath} - " + (result.Success ? "Success" : "Fail"));
    return new(result.Success);
  }
}


public class BuildClient
{
  private JsonRpc? _rpc;
  private Process? _serverProcess;
  private Task? _connectTask;
  private readonly object _connectLock = new();
  private readonly string _pipeName;

  public BuildClient(string pipeName)
  {
    Console.WriteLine("SPAWNING " + pipeName);
    _pipeName = pipeName;
  }

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
      _serverProcess = BuildServerStarter.StartBuildServer(_pipeName);
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
  public static Process StartBuildServer(string pipeName)
  {
    // string exePath;
    // var exePath = "C:/Users/Gustav/repo/easy-dotnet-server/EasyDotnet.MsBuildSdk/bin/Debug/net9.0/EasyDotnet.MsBuildSdk.exe";
    var dir = HostDirectoryUtil.HostDirectory;


#if DEBUG
    var exePath = Path.Combine(
        dir,
        "EasyDotnet.MsBuildSdk", "bin", "Debug", "net8.0", GetExecutable("EasyDotnet.MsBuildSdk"));
#else
    var exeHost = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    var exePath = Path.Combine(exeHost, "MsBuildSdk", GetExecutable("EasyDotnet.MsBuildSdk"));
#endif
    // string? exePath;
    // if (Debugger.IsAttached)
    // {
    //   exePath = Path.Combine(
    //     dir, // <-- use captured original directory here
    //     "..", "..", "..", "..", "EasyDotnet.MsBuildSdk", "bin", "Debug", "net8.0", GetExecutable("EasyDotnet.MsBuildSdk"));
    // }
    // else
    // {
    //   exePath = Path.Combine(dir, GetExecutable("EasyDotnet.MsBuildSdk"));
    // }
    Console.WriteLine(exePath);

    if (!File.Exists(exePath))
      throw new FileNotFoundException("Build server executable not found.", exePath);

    var startInfo = new ProcessStartInfo
    {
      FileName = exePath,
      Arguments = pipeName,
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      CreateNoWindow = true
    };

    var process = new Process { StartInfo = startInfo };
    process.Start();

    Console.WriteLine($"Started BuildServer from: {exePath}");

    return process;
  }

  private static string GetExecutable(string name) => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{name}.exe" : name;
}