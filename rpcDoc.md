## VsTestController

### `vstest/discover`
| Parameter | Type | Optional |
|-----------|------|----------|
| vsTestPath | string |   |
| dllPath | string |   |

**Returns:** `FileResultResponse`

### `vstest/run`
| Parameter | Type | Optional |
|-----------|------|----------|
| vsTestPath | string |   |
| dllPath | string |   |
| testIds | Guid[] |   |

**Returns:** `FileResultResponse`

---

## NugetController

### `nuget/list-sources`
_No parameters_

**Returns:** `List<NugetSourceResponse>`

### `nuget/search-packages`
| Parameter | Type | Optional |
|-----------|------|----------|
| searchTerm | string |   |
| sources | List<string> | ✅  |

**Returns:** `Task<FileResultResponse>`

---

## MtpController

### `mtp/discover`
| Parameter | Type | Optional |
|-----------|------|----------|
| testExecutablePath | string |   |

**Returns:** `Task<FileResultResponse>`

### `mtp/run`
| Parameter | Type | Optional |
|-----------|------|----------|
| testExecutablePath | string |   |
| filter | RunRequestNode[] |   |

**Returns:** `Task<FileResultResponse>`

---

## MsBuildController

### `msbuild/build`
| Parameter | Type | Optional |
|-----------|------|----------|
| request | BuildRequest |   |

**Returns:** `BuildResultResponse`

### `msbuild/query-properties`
| Parameter | Type | Optional |
|-----------|------|----------|
| request | QueryProjectPropertiesRequest |   |

**Returns:** `DotnetProjectPropertiesResponse`

---

## InitializeController

### `initialize`
| Parameter | Type | Optional |
|-----------|------|----------|
| request | InitializeRequest |   |

**Returns:** `InitializeResponse`

---

