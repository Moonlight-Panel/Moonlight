using System.Text;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using Logging.Net;
using Moonlight.App.Database.Entities;
using Moonlight.App.Repositories;
using UAParser;

namespace Moonlight.App.Services.Sessions;

public class IdentityService
{
    private readonly UserRepository UserRepository;
    private readonly CookieService CookieService;
    private readonly IHttpContextAccessor HttpContextAccessor;
    private readonly string Secret;
    
    private User? UserCache;

    public IdentityService(
        CookieService cookieService,
        UserRepository userRepository,
        IHttpContextAccessor httpContextAccessor,
        ConfigService configService)
    {
        CookieService = cookieService;
        UserRepository = userRepository;
        HttpContextAccessor = httpContextAccessor;

        Secret = configService
            .GetSection("Moonlight")
            .GetSection("Security")
            .GetValue<string>("Token");
    }

    public async Task<User?> Get()
    {
        try
        {
            if (UserCache != null)
                return UserCache;

            var token = "none";

            // Load token via http context if available
            if (HttpContextAccessor.HttpContext != null)
            {
                var request = HttpContextAccessor.HttpContext.Request;

                if (request.Cookies.ContainsKey("token"))
                {
                    token = request.Cookies["token"];
                }
            }
            else // if not we check the cookies manually via js. this may not often work
            {
                token = await CookieService.GetValue("token", "none");
            }

            if (token == "none")
            {
                return null;
            }

            if (string.IsNullOrEmpty(token))
                return null;

            var json = "";

            try
            {
                json = JwtBuilder.Create()
                    .WithAlgorithm(new HMACSHA256Algorithm())
                    .WithSecret(Secret)
                    .Decode(token);
            }
            catch (TokenExpiredException)
            {
                return null;
            }
            catch (SignatureVerificationException)
            {
                //TODO: Heavy warn and write it to the logs
                Logger.Warn("Someone tried to modify his data: " + token);
                return null;
            }
            catch (Exception e)
            {
                Logger.Warn("Error parsing and validating token");
                Logger.Warn(e);
                return null;
            }

            // To make it easier to use the json data
            var data = new ConfigurationBuilder().AddJsonStream(
                new MemoryStream(Encoding.ASCII.GetBytes(json))
            ).Build();

            var userid = data.GetValue<int>("userid");
            var user = UserRepository.Get().FirstOrDefault(y => y.Id == userid);

            if (user == null)
            {
                Logger.Warn($"Cannot find user with the id '{userid}' in the database. Maybe the user has been deleted or a token has been successfully faked by a hacker");
                return null;
            }

            var iat = data.GetValue<long>("iat", -1);

            if (iat == -1)
            {
                Logger.Debug("Legacy token found (without the time the token has been issued at)");
                return null;
            }

            var issuedAt = DateTimeOffset.FromUnixTimeSeconds(iat).DateTime;

            if (issuedAt < user.TokenValidTime.ToUniversalTime())
            {
                //TODO: Remove at publish
                //Logger.Debug($"Old token found: {issuedAt.ToShortDateString()} {issuedAt.ToShortTimeString()} Current valid token time {userData.TokenValidTime.ToUniversalTime().ToShortDateString()} {userData.TokenValidTime.ToUniversalTime().ToShortTimeString()}");
                return null;
            }

            UserCache = user;
            return UserCache;
        }
        catch (Exception e)
        {
            Logger.Warn("Error loading user");
            Logger.Warn(e);
            return null;
        }
    }

    public string GetIp()
    {
        if (HttpContextAccessor.HttpContext == null)
            return "N/A";

        if(HttpContextAccessor.HttpContext.Request.Headers.ContainsKey("X-Real-IP"))
        {
            return HttpContextAccessor.HttpContext.Request.Headers["X-Real-IP"]!;
        }
        
        return HttpContextAccessor.HttpContext.Connection.RemoteIpAddress!.ToString();
    }

    public string GetDevice()
    {
        if (HttpContextAccessor.HttpContext == null)
            return "N/A";

        try
        {
            var uaParser = Parser.GetDefault();
            var info = uaParser.Parse(HttpContextAccessor.HttpContext.Request.Headers.UserAgent);

            return $"{info.OS} - {info.Device}";
        }
        catch (Exception e)
        {
            return "UserAgent not present";
        }
    }
}