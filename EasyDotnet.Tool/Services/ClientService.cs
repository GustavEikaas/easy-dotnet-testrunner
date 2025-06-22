using System;
using EasyDotnet.Controllers.Initialize;

namespace EasyDotnet.Services;

public interface IClientService
{
  bool IsInitialized { get; set; }
  ProjectInfo? ProjectInfo { get; set; }
  ClientInfo? ClientInfo { get; set; }

  void ThrowIfNotInitialized();
}

public class ClientService : IClientService
{
  public bool IsInitialized { get; set; }
  public ProjectInfo? ProjectInfo { get; set; }
  public ClientInfo? ClientInfo { get; set; }

  public void ThrowIfNotInitialized()
  {
    if (!IsInitialized)
    {
      throw new Exception("Client has not initialized yet");
    }
  }
}