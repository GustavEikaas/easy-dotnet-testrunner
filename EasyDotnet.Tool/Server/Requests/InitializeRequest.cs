using Newtonsoft.Json;

namespace EasyDotnet.Server.Requests;

public sealed record InitializeRequest(
  [property:JsonProperty("clientInfo")]
  ClientInfo ClientInfo,
  [property:JsonProperty("projectInfo")]
  ProjectInfo ProjectInfo
  );

public sealed record ProjectInfo(
  [property:JsonProperty("rootDir")]
  string RootDir
  );

public sealed record ClientInfo(
  [property:JsonProperty("name")]
  string Name,

  [property:JsonProperty("version")]
  string? Version);