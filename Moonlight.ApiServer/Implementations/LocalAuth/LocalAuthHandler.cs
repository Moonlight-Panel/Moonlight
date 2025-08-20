using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Moonlight.ApiServer.Implementations.LocalAuth;

public class LocalAuthHandler : AuthenticationHandler<LocalAuthOptions>
{
    public LocalAuthHandler(
        IOptionsMonitor<LocalAuthOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    ) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return Task.FromResult(
            AuthenticateResult.Fail("Local authentication does not directly support AuthenticateAsync")
        );
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        await Results
            .Redirect("/api/localAuth")
            .ExecuteAsync(Context);
    }
}