using EasyDotnet.Playground.RPC.Models;

using Newtonsoft.Json;

namespace EasyDotnet.Playground.RPC.Response;

public sealed record DiscoveryResponse(
    [property: JsonProperty("changes")]
  TestNodeUpdate[] Changes);