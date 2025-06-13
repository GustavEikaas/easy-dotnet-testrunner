
namespace EasyDotnet.Controllers.Initialize;

public sealed record InitializeRequest(ClientInfo ClientInfo, ProjectInfo ProjectInfo);

public sealed record ProjectInfo(string RootDir);

public sealed record ClientInfo(string Name, string? Version);