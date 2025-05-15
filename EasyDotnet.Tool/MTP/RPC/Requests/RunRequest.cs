using System;

using EasyDotnet.MTP;

using Newtonsoft.Json;

namespace EasyDotnet.Playground.RPC.Requests;

public sealed record RunRequest(
  [property:JsonProperty("tests")]
  RunRequestNode[]? TestCases,
  [property:JsonProperty("runId")]
  Guid RunId);