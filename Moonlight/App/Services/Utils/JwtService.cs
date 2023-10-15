using JWT.Algorithms;
using JWT.Builder;
using Newtonsoft.Json;

namespace Moonlight.App.Services.Utils;

public class JwtService
{
    private readonly ConfigService ConfigService;
    private readonly TimeSpan DefaultDuration = TimeSpan.FromDays(365 * 10);

    public JwtService(ConfigService configService)
    {
        ConfigService = configService;
    }

    public Task<string> Create(Action<Dictionary<string, string>> data, TimeSpan? validDuration = null)
    {
        var builder = new JwtBuilder()
            .WithSecret(ConfigService.Get().Security.Token)
            .IssuedAt(DateTime.UtcNow)
            .ExpirationTime(DateTime.UtcNow.Add(validDuration ?? DefaultDuration))
            .WithAlgorithm(new HMACSHA512Algorithm());

        var dataDic = new Dictionary<string, string>();
        data.Invoke(dataDic);

        foreach (var entry in dataDic)
            builder = builder.AddClaim(entry.Key, entry.Value);

        var jwt = builder.Encode();
        
        return Task.FromResult(jwt);
    }

    public Task<bool> Validate(string token)
    {
        try
        {
            _ = new JwtBuilder()
                .WithSecret(ConfigService.Get().Security.Token)
                .WithAlgorithm(new HMACSHA512Algorithm())
                .MustVerifySignature()
                .Decode(token);
            
            return Task.FromResult(true);
        }
        catch (Exception e)
        {
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
        catch (Exception)
        {
            return Task.FromResult<Dictionary<string, string>>(null!);
        }
    }
}