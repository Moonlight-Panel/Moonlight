using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using MoonCore.Attributes;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;

namespace Moonlight.ApiServer.Services;

[Singleton]
public class ApiKeyService
{
    private readonly AppConfiguration Configuration;

    public ApiKeyService(AppConfiguration configuration)
    {
        Configuration = configuration;
    }

    public string GenerateJwt(ApiKey apiKey)
    {
        var permissions = JsonSerializer.Deserialize<string[]>(apiKey.PermissionsJson) ?? [];
        
        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

        var descriptor = new SecurityTokenDescriptor()
        {
            Expires = apiKey.ExpiresAt,
            IssuedAt = DateTime.Now,
            NotBefore = DateTime.Now.AddMinutes(-1),
            Claims = new Dictionary<string, object>()
            {
                {
                    "apiKeyId",
                    apiKey.Id
                },
                {
                    "permissions",
                    string.Join(";", permissions)
                }
            },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Configuration.Authentication.Secret)
                ),
                SecurityAlgorithms.HmacSha256
            ),
            Issuer = Configuration.PublicUrl,
            Audience = Configuration.PublicUrl
        };

        var securityToken = jwtSecurityTokenHandler.CreateJwtSecurityToken(descriptor);
        return jwtSecurityTokenHandler.WriteToken(securityToken);
    }
}