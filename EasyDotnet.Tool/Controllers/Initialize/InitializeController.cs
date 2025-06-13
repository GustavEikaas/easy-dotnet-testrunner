using System;
using System.IO;
using System.Reflection;
using EasyDotnet.Services;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.Initialize;

public class InitializeController(ClientService clientService) : BaseController
{
  [JsonRpcMethod("initialize")]
  public InitializeResponse Initialize(InitializeRequest request)
  {
    var assembly = Assembly.GetExecutingAssembly();
    var serverVersion = assembly.GetName().Version ?? throw new NullReferenceException("Server version");

    if (!Version.TryParse(request.ClientInfo.Version, out var clientVersion))
    {
      throw new Exception("Invalid client version format");
    }

    if (clientVersion.Major != serverVersion.Major)
    {
      if (clientVersion.Major < serverVersion.Major)
      {
        throw new Exception($"Client is outdated. Please update your client. Server Version: {serverVersion}, Client Version: {clientVersion}");
      }
      else
      {
        throw new Exception($"Server is outdated. Please update the server. `dotnet tool install -g EasyDotnet` Server Version: {serverVersion}, Client Version: {clientVersion}");
      }
    }
    Directory.SetCurrentDirectory(request.ProjectInfo.RootDir);
    clientService.IsInitialized = true;
    clientService.ProjectInfo = request.ProjectInfo;
    clientService.ClientInfo = request.ClientInfo;
    return new InitializeResponse(new ServerInfo("EasyDotnet", serverVersion.ToString()));
  }
}