using System;

namespace EasyDotnet.VSTest;

public sealed record RunRequest(
    string VsTestPath,
    string DllPath,
    Guid[] TestIds,
    string OutFile
    );