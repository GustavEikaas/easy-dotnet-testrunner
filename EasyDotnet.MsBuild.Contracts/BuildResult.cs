using System.Collections.Generic;

namespace EasyDotnet.MsBuild.Contracts
{
    public sealed class BuildResult
    {
        public bool Success { get; }
        public List<BuildMessage> Errors { get; }
        public List<BuildMessage> Warnings { get; }

        public BuildResult(bool success, List<BuildMessage> errors, List<BuildMessage> warnings)
        {
            Success = success;
            Errors = errors ?? new List<BuildMessage>();
            Warnings = warnings ?? new List<BuildMessage>();
        }
    }
}