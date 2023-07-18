using System.Text;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Helpers;
using Moonlight.App.Perms;
using Moonlight.App.Repositories;
using UAParser;

namespace Moonlight.App.Services.Sessions;

public class IdentityService
{
    private readonly Repository<User> UserRepository;
    private readonly CookieService CookieService;
    private readonly IHttpContextAccessor HttpContextAccessor;
    private readonly string Secret;

    public User User { get; private set; }
    public string Ip { get; private set; } = "N/A";
    public string Device { get; private set; } = "N/A";
    public PermissionStorage Permissions { get; private set; }
    public PermissionStorage UserPermissions { get; private set; }
    public PermissionStorage GroupPermissions { get; private set; }

    public IdentityService(
        CookieService cookieService,
        Repository<User> userRepository,
        IHttpContextAccessor httpContextAccessor,
        ConfigService configService)
    {
        CookieService = cookieService;
        UserRepository = userRepository;
        HttpContextAccessor = httpContextAccessor;

        Secret = configService
            .Get()
            .Moonlight.Security.Token;
    }

    public async Task Load()
    {
        await LoadIp();
        await LoadDevice();
        await LoadUser();
    }

    private async Task LoadUser()
    {
        try
        {
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
                return;
            }

            if (string.IsNullOrEmpty(token))
                return;

            string json;

            try
            {
                json = JwtBuilder.Create()
                    .WithAlgorithm(new HMACSHA256Algorithm())
                    .WithSecret(Secret)
                    .Decode(token);
            }
            catch (TokenExpiredException)
            {
                return;
            }
            catch (SignatureVerificationException)
            {
                Logger.Warn($"Detected a manipulated JWT: {token}", "security");
                return;
            }
            catch (Exception e)
            {
                Logger.Error("Error reading jwt");
                Logger.Error(e);
                return;
            }

            // To make it easier to use the json data
            var data = new ConfigurationBuilder().AddJsonStream(
                new MemoryStream(Encoding.ASCII.GetBytes(json))
            ).Build();

            var userid = data.GetValue<int>("userid");
            var user = UserRepository.Get().FirstOrDefault(y => y.Id == userid);

            if (user == null)
            {
                Logger.Warn(
                    $"Cannot find user with the id '{userid}' in the database. Maybe the user has been deleted or a token has been successfully faked by a hacker");
                return;
            }

            var iat = data.GetValue<long>("iat", -1);

            if (iat == -1)
            {
                Logger.Debug("Legacy token found (without the time the token has been issued at)");
                return;
            }

            var iatD = DateTimeOffset.FromUnixTimeSeconds(iat).ToUniversalTime().DateTime;

            if (iatD < user.TokenValidTime)
                return;

            User = user;

            ConstructPermissions();

            User.LastIp = Ip;
            UserRepository.Update(User);
        }
        catch (Exception e)
        {
            Logger.Error("Unexpected error while processing token");
            Logger.Error(e);
            return;
        }
    }

    private Task LoadIp()
    {
        if (HttpContextAccessor.HttpContext == null)
        {
            Ip = "N/A";
            return Task.CompletedTask;
        }

        if (HttpContextAccessor.HttpContext.Request.Headers.ContainsKey("X-Real-IP"))
        {
            Ip = HttpContextAccessor.HttpContext.Request.Headers["X-Real-IP"]!;
            return Task.CompletedTask;
        }

        Ip = HttpContextAccessor.HttpContext.Connection.RemoteIpAddress!.ToString();
        return Task.CompletedTask;
    }

    private Task LoadDevice()
    {
        if (HttpContextAccessor.HttpContext == null)
        {
            Device = "N/A";
            return Task.CompletedTask;
        }

        try
        {
            var userAgent = HttpContextAccessor.HttpContext.Request.Headers.UserAgent.ToString();

            if (userAgent.Contains("Moonlight.App"))
            {
                var version = userAgent.Remove(0, "Moonlight.App/".Length).Split(' ').FirstOrDefault();

                Device = "Moonlight App " + version;
                return Task.CompletedTask;
            }

            var uaParser = Parser.GetDefault();
            var info = uaParser.Parse(userAgent);

            Device = $"{info.OS} - {info.Device}";
            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            Device = "UserAgent not present";
            return Task.CompletedTask;
        }
    }

    public Task SavePermissions()
    {
        if (User != null)
        {
            User.Permissions = UserPermissions.Data;
            UserRepository.Update(User);
            ConstructPermissions();
        }

        return Task.CompletedTask;
    }

    private void ConstructPermissions()
    {
        if (User == null)
        {
            UserPermissions = new(Array.Empty<byte>());
            GroupPermissions = new(Array.Empty<byte>(), true);
            Permissions = new(Array.Empty<byte>(), true);

            return;
        }

        var user = UserRepository
            .Get()
            .Include(x => x.PermissionGroup)
            .First(x => x.Id == User.Id);

        UserPermissions = new PermissionStorage(user.Permissions);

        if (user.PermissionGroup == null)
            GroupPermissions = new PermissionStorage(Array.Empty<byte>(), true);
        else
            GroupPermissions = new PermissionStorage(user.PermissionGroup.Permissions, true);

        if (user.Admin)
        {
            Permissions = new PermissionStorage(Array.Empty<byte>());

            foreach (var permission in Perms.Permissions.GetAllPermissions())
            {
                Permissions[permission] = true;
            }

            Permissions.IsReadyOnly = true;
            return;
        }

        Permissions = new(Array.Empty<byte>());

        foreach (var permission in Perms.Permissions.GetAllPermissions())
        {
            Permissions[permission] = GroupPermissions[permission];
        }

        foreach (var permission in Perms.Permissions.GetAllPermissions())
        {
            if (UserPermissions[permission])
            {
                Permissions[permission] = true;
            }
        }

        Permissions.IsReadyOnly = true;
    }
}