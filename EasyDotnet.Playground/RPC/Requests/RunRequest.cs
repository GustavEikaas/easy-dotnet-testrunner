using EasyDotnet.Playground.RPC.Models;

using Newtonsoft.Json;

namespace EasyDotnet.Playground.RPC.Requests;

public sealed record RunRequest(
  [property:JsonProperty("tests")]
  TestNode[]? TestCases,
  [property:JsonProperty("runId")]
  Guid RunId);