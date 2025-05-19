using System.Text.Json;

using Newtonsoft.Json;

namespace EasyDotnet.Server.Requests;

public sealed record BuildRequest(
  [property:JsonProperty("targetPath")]
  string TargetPath,
  [property:JsonProperty("configuration")]
  string Configuration = "Debug"
);