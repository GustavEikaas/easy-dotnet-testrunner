using EasyDotnet.Services;
using StreamJsonRpc;

namespace EasyDotnet.Controllers.UserSecrets;

public class UserSecretsController(UserSecretsService userSecretsService) : BaseController
{
  [JsonRpcMethod("user-secrets/init")]
  public ProjectUserSecretsInitResponse InitSecrets(string projectPath)
  {
    var secret = userSecretsService.AddUserSecretsId(projectPath);
    return new(secret.Id, secret.FilePath);
  }

}