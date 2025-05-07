// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

using EasyDotnet.MTP;

using Newtonsoft.Json;

namespace Microsoft.Testing.Platform.ServerMode.IntegrationTests.Messages.V100;

public sealed record RunRequest(
    [property:JsonProperty("tests")]
    RunRequestNode[]? TestCases,
    [property:JsonProperty("runId")]
    Guid RunId);