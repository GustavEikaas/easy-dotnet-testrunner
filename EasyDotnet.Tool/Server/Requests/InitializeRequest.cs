using Newtonsoft.Json;

namespace EasyDotnet.Server.Requests;

public sealed record InitializeRequest(
  [property:JsonProperty("clientInfo")]
  ClientInfo ClientInfo);


public sealed record ClientInfo(
  [property:JsonProperty("name")]
  string Name,

  [property:JsonProperty("version")]
  string? Version);