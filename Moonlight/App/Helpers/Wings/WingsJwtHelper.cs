using System.Security.Cryptography;
using System.Text;
using JWT.Algorithms;
using JWT.Builder;
using Moonlight.App.Services;

namespace Moonlight.App.Helpers.Wings;

public class WingsJwtHelper
{
    private readonly ConfigService ConfigService;
    private readonly string AppUrl;

    public WingsJwtHelper(ConfigService configService)
    {
        ConfigService = configService;

        AppUrl = ConfigService.GetSection("Moonlight").GetValue<string>("AppUrl");
    }

    public string Generate(string secret, Action<Dictionary<string, string>> claimsAction)
    {
        var userid = 1;

        using MD5 md5 = MD5.Create();
        var inputBytes = Encoding.ASCII.GetBytes(userid + Guid.NewGuid().ToString());
        var outputBytes = md5.ComputeHash(inputBytes);

        var identifier = Convert.ToHexString(outputBytes).ToLower();
        var weirdId = StringHelper.GenerateString(16);

        var builder = JwtBuilder.Create()
            .AddHeader("jti", identifier)
            .WithAlgorithm(new HMACSHA256Algorithm())
            .WithSecret(secret)
            .AddClaim("user_id", userid)
            .AddClaim("jti", identifier)
            .AddClaim("unique_id", weirdId)
            .AddClaim("iat", DateTimeOffset.Now.ToUnixTimeSeconds())
            .AddClaim("nbf", DateTimeOffset.Now.AddSeconds(-10).ToUnixTimeSeconds())
            .AddClaim("exp", DateTimeOffset.Now.AddMinutes(10).ToUnixTimeSeconds())
            .AddClaim("iss", AppUrl)
            .MustVerifySignature();

        var additionalClaims = new Dictionary<string, string>();

        claimsAction.Invoke(additionalClaims);

        foreach (var claim in additionalClaims)
        {
            builder = builder.AddClaim(claim.Key, claim.Value);
        }

        return builder.Encode();
    }
}