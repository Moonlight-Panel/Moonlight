using System.Text;
using JWT.Algorithms;
using JWT.Builder;
using Moonlight.App.Exceptions;
using Moonlight.App.Helpers;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services;

public class OneTimeJwtService
{
    private readonly ConfigService ConfigService;
    private readonly RevokeRepository RevokeRepository;

    public OneTimeJwtService(ConfigService configService, RevokeRepository revokeRepository)
    {
        ConfigService = configService;
        RevokeRepository = revokeRepository;
    }

    public string Generate(Action<Dictionary<string, string>> options, TimeSpan? validTime = null)
    {
        var opt = new Dictionary<string, string>();
        options.Invoke(opt);

        string secret = ConfigService
            .GetSection("Moonlight")
            .GetSection("Security")
            .GetValue<string>("Token");

        var id = StringHelper.GenerateString(16);
        
        var builder = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA256Algorithm())
            .WithSecret(secret)
            .AddClaim("unique_id", id)
            .AddClaim("iat", DateTimeOffset.Now.ToUnixTimeSeconds())
            .AddClaim("nbf", DateTimeOffset.Now.AddSeconds(-10).ToUnixTimeSeconds())
            .MustVerifySignature();

        if (validTime == null)
            builder = builder.AddClaim("exp", DateTimeOffset.Now.AddMinutes(10).ToUnixTimeSeconds());
        else
            builder = builder.AddClaim("exp", DateTimeOffset.Now.Add(validTime.Value).ToUnixTimeSeconds());

        foreach (var o in opt)
        {
            builder = builder.AddClaim(o.Key, o.Value);
        }

        return builder.Encode();
    }

    public Dictionary<string, string>? Validate(string token)
    {
        string secret = ConfigService
            .GetSection("Moonlight")
            .GetSection("Security")
            .GetValue<string>("Token");

        string json;
        
        try
        {
            json = JwtBuilder.Create()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(secret)
                .Decode(token);
            
            //TODO: Error handling, report signature errors
        }
        catch (Exception e)
        {
            return null;
        }

        var data = new ConfigurationBuilder().AddJsonStream(
            new MemoryStream(Encoding.ASCII.GetBytes(json))
        ).Build();

        var id = data.GetValue<string>("unique_id");

        if (RevokeRepository
                .Get()
                .FirstOrDefault(x => x.Identifier == id) != null)
        {
            throw new DisplayException("This token has been already used");
        }

        var opt = new Dictionary<string, string>();

        foreach (var child in data.GetChildren())
        {
            opt.Add(child.Key, child.Value!);
        }

        return opt;
    }

    public void Revoke(string token)
    {
        var values = Validate(token);

        RevokeRepository.Add(new()
        {
            Identifier = values["unique_id"]
        });
    }
}