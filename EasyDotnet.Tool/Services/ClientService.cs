using EasyDotnet.Controllers.Initialize;

namespace EasyDotnet.Services;

public class ClientService
{
  public bool IsInitialized { get; set; }
  public ProjectInfo? ProjectInfo { get; set; }
  public ClientInfo? ClientInfo { get; set; }
}