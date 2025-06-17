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

## UserSecretsController

### `user-secrets/init`
| Parameter | Type | Optional |
|-----------|------|----------|
| projectPath | string |   |

**Returns:** `ProjectUserSecretsInitResponse`

---

## SolutionController

### `solution/list-projects`
| Parameter | Type | Optional |
|-----------|------|----------|
| solutionFilePath | string |   |

**Returns:** `List<SolutionFileProjectResponse>`

---

## NugetController

### `nuget/list-sources`
_No parameters_

**Returns:** `List<NugetSourceResponse>`

### `nuget/push`
| Parameter | Type | Optional |
|-----------|------|----------|
| packagePaths | List<string> |   |
| source | string |   |
| apiKey | string | ✅  |

**Returns:** `Task<NugetPushResponse>`

### `nuget/get-package-versions`
| Parameter | Type | Optional |
|-----------|------|----------|
| packageId | string |   |
| sources | List<string> | ✅  |
| includePrerelease | bool | ✅  |

**Returns:** `Task<List<string>>`

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

### `msbuild/restore`
| Parameter | Type | Optional |
|-----------|------|----------|
| targetPath | string |   |

**Returns:** `BuildResultResponse`

### `msbuild/pack`
| Parameter | Type | Optional |
|-----------|------|----------|
| targetPath | string |   |
| configuration | string |   |

**Returns:** `PackResultResponse`

### `msbuild/query-properties`
| Parameter | Type | Optional |
|-----------|------|----------|
| request | QueryProjectPropertiesRequest |   |

**Returns:** `DotnetProjectPropertiesResponse`

### `msbuild/add-package-reference`
| Parameter | Type | Optional |
|-----------|------|----------|
| targetPath | string |   |
| packageName | string |   |

**Returns:** `Task`

---

## InitializeController

### `initialize`
| Parameter | Type | Optional |
|-----------|------|----------|
| request | InitializeRequest |   |

**Returns:** `InitializeResponse`

---

