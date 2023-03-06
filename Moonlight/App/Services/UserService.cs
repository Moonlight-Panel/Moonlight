using JWT.Algorithms;
using JWT.Builder;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories;
using Moonlight.App.Services.LogServices;

namespace Moonlight.App.Services;

public class UserService
{
    private readonly UserRepository UserRepository;
    private readonly TotpService TotpService;
    private readonly SecurityLogService SecurityLogService;
    private readonly AuditLogService AuditLogService;

    private readonly string JwtSecret;

    public UserService(
        UserRepository userRepository,
        TotpService totpService,
        ConfigService configService, 
        SecurityLogService securityLogService,
        AuditLogService auditLogService)
    {
        UserRepository = userRepository;
        TotpService = totpService;
        SecurityLogService = securityLogService;
        AuditLogService = auditLogService;

        JwtSecret = configService
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
        
        await AuditLogService.Log(AuditLogType.Register, user.Email);

        return await GenerateToken(user);
    }

    public async Task<bool> CheckTotp(string email, string password)
    {
        var user = UserRepository.Get()
            .FirstOrDefault(
                x => x.Email == email
            );

        if (user == null)
        {
            await SecurityLogService.Log(SecurityLogType.LoginFail, new[] { email, password });
            throw new DisplayException("Email and password combination not found");
        }

        if (BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return user.TotpEnabled;
        }

        await SecurityLogService.Log(SecurityLogType.LoginFail, new[] { email, password });
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

            var totpCodeValid = await TotpService.Verify(user!.TotpSecret, totpCode);

            if (totpCodeValid)
            {
                await AuditLogService.Log(AuditLogType.Login, email);
                return await GenerateToken(user);
            }
            else
            {
                await SecurityLogService.Log(SecurityLogType.LoginFail, new[] { email, password });
                throw new DisplayException("2FA code invalid");
            }
        }
        else
        {
            await AuditLogService.Log(AuditLogType.Login, email);
            return await GenerateToken(user!);
        }
    }

    public async Task ChangePassword(User user, string password)
    {
        user.Password = BCrypt.Net.BCrypt.HashPassword(password);
        user.TokenValidTime = DateTime.Now;
        UserRepository.Update(user);

        //var mail = new NewPasswordMail(user);
        //await MailService.Send(mail, user);

        await AuditLogService.Log(AuditLogType.ChangePassword, user.Email);
    }

    public async Task<User> SftpLogin(int id, string password)
    {
        var user = UserRepository.Get().FirstOrDefault(x => x.Id == id);

        if (user == null)
        {
            await SecurityLogService.LogSystem(SecurityLogType.SftpBruteForce, id);
            throw new Exception("Invalid username");
        }
        
        if (BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            await AuditLogService.LogSystem(AuditLogType.Login, user.Email);
            return user;
        }
        
        await SecurityLogService.LogSystem(SecurityLogType.SftpBruteForce, new[] { id.ToString(), password });
        throw new Exception("Invalid userid or password");
    }

    public Task<string> GenerateToken(User user)
    {
        var token = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA256Algorithm())
            .WithSecret(JwtSecret)
            .AddClaim("exp", DateTimeOffset.UtcNow.AddDays(10).ToUnixTimeSeconds())
            .AddClaim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            .AddClaim("userid", user.Id)
            .Encode();

        return Task.FromResult(token);
    }
}