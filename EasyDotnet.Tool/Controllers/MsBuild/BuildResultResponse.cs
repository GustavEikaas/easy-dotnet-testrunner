using System.Collections.Generic;

namespace EasyDotnet.Controllers.MsBuild;

public sealed record BuildResultResponse(bool Success, List<string> Errors);