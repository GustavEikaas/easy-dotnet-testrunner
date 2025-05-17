using Newtonsoft.Json;

namespace EasyDotnet.Server.Responses;

public sealed record ServerInfo(
[property:JsonProperty("name")]
string Name,

[property:JsonProperty("version")]
string Version);

public sealed record InitializeResponse(
  ServerInfo ServerInfo);