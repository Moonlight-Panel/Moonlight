using JWT.Algorithms;
using JWT.Builder;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services;

public class UserService
{
    private readonly UserRepository UserRepository;
    private readonly TotpService TotpService;
    private readonly ConfigService ConfigService;

    private readonly string JwtSecret;

    public UserService(
        UserRepository userRepository,
        TotpService totpService,
        ConfigService configService)
    {
        UserRepository = userRepository;
        TotpService = totpService;
        ConfigService = configService;

        JwtSecret = ConfigService
            .GetSection("Moonlight")
            .GetSection("Security")
            .GetValue<string>("Token");
    }

    public async Task<string> Register(string email, string password, string firstname, string lastname)
    {
        var emailTaken = UserRepository.Get().FirstOrDefault(x => x.Email == email) != null;

        if (emailTaken)
        {
            //AuditLogService.Log("register:fail", $"Invalid email: {email}");
            throw new DisplayException("The email is already in use");
        }

        var user = UserRepository.Add(new()
        {
            Address = "",
            Admin = false,
            City = "",
            Country = "",
            Email = email,
            Password = BCrypt.Net.BCrypt.HashPassword(password),
            FirstName = firstname,
            LastName = lastname,
            State = "",
            Status = UserStatus.Unverified,
            CreatedAt = DateTime.Now,
            DiscordDiscriminator = "",
            DiscordId = -1,
            DiscordUsername = "",
            TotpEnabled = false,
            TotpSecret = "",
            UpdatedAt = DateTime.Now,
            TokenValidTime = DateTime.Now.AddDays(-5)
        });

        //AuditLogService.Log("register:done", $"A new user has registered: Email: {email}");

        //var mail = new WelcomeMail(user);
        //await MailService.Send(mail, user);

        return JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA256Algorithm())
            .WithSecret(JwtSecret)
            .AddClaim("exp", DateTimeOffset.UtcNow.AddDays(10).ToUnixTimeSeconds())
            .AddClaim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            .AddClaim("userid", user.Id)
            .Encode();
    }

    public Task<bool> CheckTotp(string email, string password)
    {
        var user = UserRepository.Get()
            .FirstOrDefault(
                x => x.Email.Equals(
                    email
                )
            );

        if (user == null)
        {
            //AuditLogService.Log("login:fail", $"Invalid email: {email}. Password: {password}");
            throw new DisplayException("Email and password combination not found");
        }

        if (BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return Task.FromResult(user.TotpEnabled);
        }

        //AuditLogService.Log("login:fail", $"Invalid email: {email}. Password: {password}");
        throw new DisplayException("Email and password combination not found");;
    }

    public async Task<string> Login(string email, string password, string totpCode = "")
    {
        var needTotp = await CheckTotp(email, password);
        
        var user = UserRepository.Get()
            .FirstOrDefault(
                x => x.Email.Equals(
                    email
                )
            );

        if (needTotp)
        {
            if (string.IsNullOrEmpty(totpCode))
                throw new DisplayException("2FA code must be provided");

            var totpCodeValid = await TotpService.Verify(user.TotpSecret, totpCode);

            if (totpCodeValid)
            {
                //AuditLogService.Log("login:success", $"{user.Email} has successfully logged in");

                return JwtBuilder.Create()
                    .WithAlgorithm(new HMACSHA256Algorithm())
                    .WithSecret(JwtSecret)
                    .AddClaim("exp", DateTimeOffset.UtcNow.AddDays(10).ToUnixTimeSeconds())
                    .AddClaim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    .AddClaim("userid", user.Id)
                    .Encode();
            }
            else
            {
                //AuditLogService.Log("login:fail", $"Invalid totp code: {totpCode}");
                throw new DisplayException("2FA code invalid");
            }
        }
        else
        {
            //AuditLogService.Log("login:success", $"{user.Email} has successfully logged in");

            return JwtBuilder.Create()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(JwtSecret)
                .AddClaim("exp", DateTimeOffset.UtcNow.AddDays(10).ToUnixTimeSeconds())
                .AddClaim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                .AddClaim("userid", user.Id)
                .Encode();
        }
    }

    public async Task ChangePassword(User user, string password)
    {
        user.Password = BCrypt.Net.BCrypt.HashPassword(password);
        user.TokenValidTime = DateTime.Now;
        UserRepository.Update(user);

        //var mail = new NewPasswordMail(user);
        //await MailService.Send(mail, user);
            
        //AuditLogService.Log("password:change", "The password has been set to a new one");
    }

    public Task<User> SftpLogin(int id, string password)
    {
        var user = UserRepository.Get().FirstOrDefault(x => x.Id == id);

        if (user == null)
            throw new Exception("Unknown user");
        
        if (BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            //TODO: Maybe log
            return Task.FromResult(user);
        }
        
        //TODO: Log
        throw new Exception("Invalid userid or password");
    }
}