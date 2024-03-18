using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using MoonCore.Attributes;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.Core.Configuration;
using Newtonsoft.Json;

namespace Moonlight.Core.Services.Utils;

[Singleton]
public class JwtService
{
    private readonly ConfigService<ConfigV1> ConfigService;
    private readonly TimeSpan DefaultDuration = TimeSpan.FromDays(365 * 10);

    public JwtService(ConfigService<ConfigV1> configService)
    {
        ConfigService = configService;
    }

    public Task<string> Create(Action<Dictionary<string, string>> data, string type, TimeSpan? validDuration = null)
    {
        var builder = new JwtBuilder()
            .WithSecret(ConfigService.Get().Security.Token)
            .IssuedAt(DateTime.UtcNow)
            .AddHeader("Type", type)
            .ExpirationTime(DateTime.UtcNow.Add(validDuration ?? DefaultDuration))
            .WithAlgorithm(new HMACSHA512Algorithm());

        var dataDic = new Dictionary<string, string>();
        data.Invoke(dataDic);

        foreach (var entry in dataDic)
            builder = builder.AddClaim(entry.Key, entry.Value);

        var jwt = builder.Encode();
        
        return Task.FromResult(jwt);
    }

    public Task<bool> Validate(string token, params string[] allowedJwtTypes)
    {
        try
        {
            // Without the body decode call the jwt validation would not work for some weird reason.
            // It would not throw an error when the signature is invalid
            _ = new JwtBuilder()
                .WithSecret(ConfigService.Get().Security.Token)
                .WithAlgorithm(new HMACSHA512Algorithm())
                .MustVerifySignature()
                .Decode(token);
            
            var headerJson = new JwtBuilder()
                .WithSecret(ConfigService.Get().Security.Token)
                .WithAlgorithm(new HMACSHA512Algorithm())
                .MustVerifySignature()
                .DecodeHeader(token);

            if (headerJson == null)
                return Task.FromResult(false);

            // Jwt type validation
            if (allowedJwtTypes.Length == 0)
                return Task.FromResult(true);

            var headerData = JsonConvert.DeserializeObject<Dictionary<string, string>>(headerJson);

            if (headerData == null) // => Invalid header
                return Task.FromResult(false);

            if (!headerData.ContainsKey("Type")) // => Invalid header, Type is missing
                return Task.FromResult(false);

            foreach (var name in allowedJwtTypes)
            {
                if (headerData["Type"] == name) // => Correct type found
                    return Task.FromResult(true);
            }

            // None found? Invalid type!
            return Task.FromResult(false);
        }
        catch (SignatureVerificationException)
        {
            Logger.Warn($"A manipulated jwt has been found. Required jwt types: {string.Join(" ", allowedJwtTypes)} Jwt: {token}");
            
            return Task.FromResult(false);
        }
        catch (Exception e)
        {
            Logger.Warn(e.Message);
            return Task.FromResult(false);
        }
    }

    public Task<Dictionary<string, string>> Decode(string token)
    {
        try
        {
            var json = new JwtBuilder()
                .WithSecret(ConfigService.Get().Security.Token)
                .WithAlgorithm(new HMACSHA512Algorithm())
                .MustVerifySignature()
                .Decode(token);

            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            return Task.FromResult(data)!;
        }
        catch (SignatureVerificationException)
        {
            return Task.FromResult(new Dictionary<string, string>());
        }
        catch (Exception e)
        {
            Logger.Warn("An unknown error occured while processing token");
            Logger.Warn(e);
            return Task.FromResult<Dictionary<string, string>>(null!);
        }
    }
}