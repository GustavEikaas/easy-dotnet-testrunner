// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;

namespace EasyDotnet.MTP.ServerMode;

public sealed record AttachDebuggerInfo(
    [property:JsonProperty("processId")]
    int ProcessId);