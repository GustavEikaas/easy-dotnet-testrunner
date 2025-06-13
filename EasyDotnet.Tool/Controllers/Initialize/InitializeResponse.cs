namespace EasyDotnet.Controllers.Initialize;

public sealed record ServerInfo(string Name, string Version);

public sealed record InitializeResponse(ServerInfo ServerInfo);