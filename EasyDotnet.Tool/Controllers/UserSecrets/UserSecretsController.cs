using EasyDotnet.Services;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.UserSecrets;

public class UserSecretsController(UserSecretsService userSecretsService) : BaseController
{

  [JsonRpcMethod("user-secrets/init")]
  public ProjectUserSecretResponse InitSecrets(string projectPath)
  {
    var secret = userSecretsService.AddUserSecretsId(projectPath);
    return new(secret.Id, secret.FilePath);
  }

}

public sealed record ProjectUserSecretResponse(string Id, string FilePath);