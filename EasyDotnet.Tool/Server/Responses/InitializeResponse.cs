using Newtonsoft.Json;

namespace EasyDotnet.Server.Responses;

public sealed record ServerInfo(string Name, string Version);

public sealed record InitializeResponse(ServerInfo ServerInfo);