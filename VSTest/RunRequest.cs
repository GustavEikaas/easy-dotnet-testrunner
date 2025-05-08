namespace EasyDotnet.VSTest;

public sealed record RunRequest(
    string VsTestPath,
    string DllPath,
    string Filter,
    string OutFile
    );