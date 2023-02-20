using System.Security.Cryptography;
using System.Text;
using JWT.Algorithms;
using JWT.Builder;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Repositories.Servers;
using Moonlight.App.Services;

namespace Moonlight.App.Helpers;

public class WingsConsoleHelper
{
    private readonly ServerRepository ServerRepository;
    private readonly WingsJwtHelper WingsJwtHelper;
    private readonly string AppUrl;

    public WingsConsoleHelper(
        ServerRepository serverRepository,
        ConfigService configService,
        WingsJwtHelper wingsJwtHelper)
    {
        ServerRepository = serverRepository;
        WingsJwtHelper = wingsJwtHelper;

        AppUrl = configService.GetSection("Moonlight").GetValue<string>("AppUrl");
    }

    public async Task ConnectWings(PteroConsole.NET.PteroConsole pteroConsole, Server server)
    {
        var serverData = ServerRepository
            .Get()
            .Include(x => x.Node)
            .First(x => x.Id == server.Id);

        var token = GenerateToken(serverData);

        if (serverData.Node.Ssl)
        {
            await pteroConsole.Connect(
                AppUrl,
                $"wss://{serverData.Node.Fqdn}:{serverData.Node.HttpPort}/api/servers/{serverData.Uuid}/ws",
                token
            );
        }
        else
        {
            await pteroConsole.Connect(
                AppUrl,
                $"ws://{serverData.Node.Fqdn}:{serverData.Node.HttpPort}/api/servers/{serverData.Uuid}/ws",
                token
            );
        }
    }

    public string GenerateToken(Server server)
    {
        var serverData = ServerRepository
            .Get()
            .Include(x => x.Node)
            .First(x => x.Id == server.Id);

        var userid = 1;
        var secret = serverData.Node.Token;
        

        using (MD5 md5 = MD5.Create())
        {
            var inputBytes = Encoding.ASCII.GetBytes(userid + serverData.Uuid.ToString());
            var outputBytes = md5.ComputeHash(inputBytes);

            var identifier = Convert.ToHexString(outputBytes).ToLower();
            var weirdId = StringHelper.GenerateString(16);

            var token = JwtBuilder.Create()
                .AddHeader("jti", identifier)
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(secret)
                .AddClaim("user_id", userid)
                .AddClaim("server_uuid", serverData.Uuid.ToString())
                .AddClaim("permissions", new[]
                {
                    "*",
                    "admin.websocket.errors",
                    "admin.websocket.install",
                    "admin.websocket.transfer"
                })
                .AddClaim("jti", identifier)
                .AddClaim("unique_id", weirdId)
                .AddClaim("iat", DateTimeOffset.Now.ToUnixTimeSeconds())
                .AddClaim("nbf", DateTimeOffset.Now.AddSeconds(-10).ToUnixTimeSeconds())
                .AddClaim("exp", DateTimeOffset.Now.AddMinutes(10).ToUnixTimeSeconds())
                .AddClaim("iss", AppUrl)
                .AddClaim("aud", new[]
                {
                    serverData.Node.Ssl ? $"https://{serverData.Node.Fqdn}" : $"http://{serverData.Node.Fqdn}"
                })
                .MustVerifySignature()
                .Encode();

            return token;
        }
    }
}