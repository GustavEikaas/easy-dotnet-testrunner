using System;

using Newtonsoft.Json;

namespace EasyDotnet.Playground.RPC.Requests;

public sealed record DiscoveryRequest(
    [property:JsonProperty("runId")]
  Guid RunId);