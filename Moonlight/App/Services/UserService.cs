using JWT.Algorithms;
using JWT.Builder;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories;
using Moonlight.App.Services.LogServices;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Services;

public class UserService
{
    private readonly UserRepository UserRepository;
    private readonly TotpService TotpService;
    private readonly SecurityLogService SecurityLogService;
    private readonly AuditLogService AuditLogService;
    private readonly MailService MailService;
    private readonly IdentityService IdentityService;
    private readonly IpLocateService IpLocateService;
    private readonly DateTimeService DateTimeService;

    private readonly string JwtSecret;

    public UserService(
        UserRepository userRepository,
        TotpService totpService,
        ConfigService configService, 
        SecurityLogService securityLogService,
        AuditLogService auditLogService,
        MailService mailService,
        IdentityService identityService,
        IpLocateService ipLocateService,
        DateTimeService dateTimeService)
    {
        UserRepository = userRepository;
        TotpService = totpService;
        SecurityLogService = securityLogService;
        AuditLogService = auditLogService;
        MailService = mailService;
        IdentityService = identityService;
        IpLocateService = ipLocateService;
        DateTimeService = dateTimeService;

        JwtSecret = configService
            .GetSection("Moonlight")
            .GetSection("Security")
            .GetValue<string>("Token");
    }

    public async Task<string> Register(string email, string password, string firstname, string lastname)
    {
        // Check if the email is already taken
        var emailTaken = UserRepository.Get().FirstOrDefault(x => x.Email == email) != null;

        if (emailTaken)
        {
            throw new DisplayException("The email is already in use");
        }
        
        //TODO: Validation

        // Add user
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
            CreatedAt = DateTimeService.GetCurrent(),
            DiscordId = 0,
            TotpEnabled = false,
            TotpSecret = "",
            UpdatedAt = DateTimeService.GetCurrent(),
            TokenValidTime = DateTimeService.GetCurrent().AddDays(-5)
        });

        await MailService.SendMail(user!, "register", values => {});
        
        await AuditLogService.Log(AuditLogType.Register, x =>
        {
            x.Add<User>(user.Email);
        });

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
            await SecurityLogService.Log(SecurityLogType.LoginFail, x =>
            {
                x.Add<User>(email);
                x.Add<string>(password);
            });
            throw new DisplayException("Email and password combination not found");
        }

        if (BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return user.TotpEnabled;
        }

        await SecurityLogService.Log(SecurityLogType.LoginFail, x =>
        {
            x.Add<User>(email);
            x.Add<string>(password);
        });
        throw new DisplayException("Email and password combination not found");;
    }

    public async Task<string> Login(string email, string password, string totpCode = "")
    {
        // First password check and check if totp is enabled
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
                await AuditLogService.Log(AuditLogType.Login, x =>
                {
                    x.Add<User>(email);
                });
                return await GenerateToken(user, true);
            }
            else
            {
                await SecurityLogService.Log(SecurityLogType.LoginFail, x =>
                {
                    x.Add<User>(email);
                    x.Add<string>(password);
                });
                throw new DisplayException("2FA code invalid");
            }
        }
        else
        {
            await AuditLogService.Log(AuditLogType.Login, x =>
            {
                x.Add<User>(email);
            });
            return await GenerateToken(user!, true);
        }
    }

    public async Task ChangePassword(User user, string password, bool isSystemAction = false)
    {
        user.Password = BCrypt.Net.BCrypt.HashPassword(password);
        user.TokenValidTime = DateTimeService.GetCurrent();
        UserRepository.Update(user);

        if (isSystemAction)
        {
            await AuditLogService.LogSystem(AuditLogType.ChangePassword, x=>
            {
                x.Add<User>(user.Email);
            });
        }
        else
        {
            var location = await IpLocateService.GetLocation();
            
            await MailService.SendMail(user!, "passwordChange", values =>
            {
                values.Add("Ip", IdentityService.GetIp());
                values.Add("Device", IdentityService.GetDevice());
                values.Add("Location", location);
            });

            await AuditLogService.Log(AuditLogType.ChangePassword, x =>
            {
                x.Add<User>(user.Email);
            });
        }
    }

    public async Task<User> SftpLogin(int id, string password)
    {
        var user = UserRepository.Get().FirstOrDefault(x => x.Id == id);

        if (user == null)
        {
            await SecurityLogService.LogSystem(SecurityLogType.SftpBruteForce, x =>
            {
                x.Add<int>(id);
            });
            
            throw new Exception("Invalid username");
        }
        
        if (BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            await AuditLogService.LogSystem(AuditLogType.Login, x =>
            {
                x.Add<User>(user.Email);
            });
            return user;
        }
        
        await SecurityLogService.LogSystem(SecurityLogType.SftpBruteForce, x =>
        {
            x.Add<int>(id);
            x.Add<string>(password);
        });
        throw new Exception("Invalid userid or password");
    }

    public async Task<string> GenerateToken(User user, bool sendMail = false)
    {
        var location = await IpLocateService.GetLocation();

        if (sendMail)
        {
            await MailService.SendMail(user!, "login", values =>
            {
                values.Add("Ip", IdentityService.GetIp());
                values.Add("Device", IdentityService.GetDevice());
                values.Add("Location", location);
            });
        }
        
        var token = JwtBuilder.Create()
            .WithAlgorithm(new HMACSHA256Algorithm())
            .WithSecret(JwtSecret)
            .AddClaim("exp", new DateTimeOffset(DateTimeService.GetCurrent().AddDays(10)).ToUnixTimeSeconds())
            .AddClaim("iat", DateTimeService.GetCurrentUnixSeconds())
            .AddClaim("userid", user.Id)
            .Encode();

        return token;
    }

    public async Task ResetPassword(string email)
    {
        email = email.ToLower();
        
        var user = UserRepository
            .Get()
            .FirstOrDefault(x => x.Email == email);

        if (user == null)
            throw new DisplayException("A user with this email can not be found");

        var newPassword = StringHelper.GenerateString(16);
        await ChangePassword(user, newPassword, true);

        await AuditLogService.Log(AuditLogType.PasswordReset, x => {});

        var location = await IpLocateService.GetLocation();

        await MailService.SendMail(user, "passwordReset", values =>
        {
            values.Add("Ip", IdentityService.GetIp());
            values.Add("Device", IdentityService.GetDevice());
            values.Add("Location", location);
            values.Add("Password", newPassword);
        });
    }
}