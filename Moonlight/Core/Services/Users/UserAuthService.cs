using MoonCore.Abstractions;
using MoonCore.Attributes;
using MoonCore.Exceptions;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.Core.Configuration;
using Moonlight.Core.Database.Entities;
using Moonlight.Core.Event;
using Moonlight.Core.Extensions;
using Moonlight.Core.Models.Abstractions;
using Moonlight.Core.Models.Enums;
using Moonlight.Core.Models.Templates;
using Moonlight.Core.Services.Utils;
using OtpNet;

namespace Moonlight.Core.Services.Users;

[Scoped]
public class UserAuthService
{
    private readonly Repository<User> UserRepository;
    private readonly JwtService JwtService;
    private readonly ConfigService<ConfigV1> ConfigService;
    private readonly MailService MailService;

    public UserAuthService(
        Repository<User> userRepository,
        JwtService jwtService,
        ConfigService<ConfigV1> configService,
        MailService mailService)
    {
        UserRepository = userRepository;
        JwtService = jwtService;
        ConfigService = configService;
        MailService = mailService;
    }

    public async Task<User> Register(string username, string email, string password)
    {
        // Event though we have form validation i want to
        // ensure that at least these basic formatting things are done
        email = email.ToLower().Trim();
        username = username.ToLower().Trim();

        // Prevent duplication or username and/or email
        if (UserRepository.Get().Any(x => x.Email == email))
            throw new DisplayException("A user with that email does already exist");

        if (UserRepository.Get().Any(x => x.Username == username))
            throw new DisplayException("A user with that username does already exist");

        var user = new User()
        {
            Username = username,
            Email = email,
            Password = HashHelper.HashToString(password)
        };

        var result = UserRepository.Add(user);

        await Events.OnUserRegistered.InvokeAsync(result);

        return result;
    }

    public async Task ChangePassword(User user, string newPassword)
    {
        user.Password = HashHelper.HashToString(newPassword);
        user.TokenValidTimestamp = DateTime.UtcNow;
        UserRepository.Update(user);

        await Events.OnUserPasswordChanged.InvokeAsync(user);
    }

    public Task SeedTotp(User user)
    {
        var key = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));

        user.TotpKey = key;
        UserRepository.Update(user);

        return Task.CompletedTask;
    }

    public async Task SetTotp(User user, bool state)
    {
        // Access to flags without identity service
        var flags = new FlagStorage(user.Flags);
        flags[UserFlag.TotpEnabled] = state;
        user.Flags = flags.RawFlagString;

        if (!state)
            user.TotpKey = null;

        UserRepository.Update(user);

        await Events.OnUserTotpSet.InvokeAsync(user);
    }

    // Mails

    public async Task SendVerification(User user)
    {
        var jwt = await JwtService.Create(data =>
        {
            data.Add("mailToVerify", user.Email);
        }, "EmailVerification", TimeSpan.FromMinutes(10));

        await MailService.Send(user, "Verify your account", "verifyMail", user, new MailVerify()
        {
            Url = ConfigService.Get().AppUrl + "/api/auth/verify?token=" + jwt
        });
    }

    public async Task SendResetPassword(string email)
    {
        var user = UserRepository
            .Get()
            .FirstOrDefault(x => x.Email == email);

        if (user == null)
            throw new DisplayException("An account with that email was not found");

        var jwt = await JwtService.Create(data =>
        {
            data.Add("accountToReset", user.Id.ToString());
        }, "PasswordReset", TimeSpan.FromHours(1));
        
        await MailService.Send(user, "Password reset for your account", "passwordReset", user, new ResetPassword()
        {
            Url = ConfigService.Get().AppUrl + "/api/auth/reset?token=" + jwt
        });
    }
}