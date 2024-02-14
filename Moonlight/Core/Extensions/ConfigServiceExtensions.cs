using MoonCore.Services;
using Moonlight.Core.Configuration;
using Newtonsoft.Json;

namespace Moonlight.Core.Extensions;

public static class ConfigServiceExtensions
{
    public static string GetDiagnosticJson(this ConfigService<ConfigV1> configService)
    {
        var jsonUnsafe = JsonConvert.SerializeObject(configService.Get());
        var configUnsafe = JsonConvert.DeserializeObject<ConfigV1>(jsonUnsafe)!;

        // Remote sensitive data
        configUnsafe.Database.Password =
            string.IsNullOrEmpty(configUnsafe.Database.Password) ? "IS EMPTY" : "IS NOT EMPTY";
        configUnsafe.Security.Token = string.IsNullOrEmpty(configUnsafe.Security.Token) ? "IS EMPTY" : "IS NOT EMPTY";
        configUnsafe.MailServer.Password =string.IsNullOrEmpty(configUnsafe.MailServer.Password) ? "IS EMPTY" : "IS NOT EMPTY";

        return JsonConvert.SerializeObject(configUnsafe, Formatting.Indented);
    }
}