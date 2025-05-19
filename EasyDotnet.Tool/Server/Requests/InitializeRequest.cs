using Newtonsoft.Json;

namespace EasyDotnet.Server.Requests;

public sealed record InitializeRequest(ClientInfo ClientInfo, ProjectInfo ProjectInfo);

public sealed record ProjectInfo(string RootDir);

public sealed record ClientInfo(string Name, string? Version);