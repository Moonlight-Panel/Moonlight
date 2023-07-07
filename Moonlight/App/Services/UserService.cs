using JWT.Algorithms;
using JWT.Builder;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories;
using Moonlight.App.Services.Mail;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Services;

public class UserService
{
    private readonly UserRepository UserRepository;
    private readonly TotpService TotpService;
    private readonly MailService MailService;
    private readonly IdentityService IdentityService;
    private readonly IpLocateService IpLocateService;
    private readonly DateTimeService DateTimeService;
    private readonly ConfigService ConfigService;

    private readonly string JwtSecret;

    public UserService(
        UserRepository userRepository,
        TotpService totpService,
        ConfigService configService,
        MailService mailService,
        IdentityService identityService,
        IpLocateService ipLocateService,
        DateTimeService dateTimeService)
    {
        UserRepository = userRepository;
        TotpService = totpService;
        ConfigService = configService;
        MailService = mailService;
        IdentityService = identityService;
        IpLocateService = ipLocateService;
        DateTimeService = dateTimeService;

        JwtSecret = configService
            .Get()
            .Moonlight.Security.Token;
    }

    public async Task<string> Register(string email, string password, string firstname, string lastname)
    {
        if (ConfigService.Get().Moonlight.Auth.DenyRegister)
            throw new DisplayException("This operation was disabled");
        
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
            TokenValidTime = DateTimeService.GetCurrent().AddDays(-5),
            LastIp = IdentityService.GetIp(),
            RegisterIp = IdentityService.GetIp()
        });

        await MailService.SendMail(user!, "register", values => {});
        
        //TODO: AuditLog

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
            Logger.Warn($"Failed login attempt. Email: {email} Password: {password}", "security");
            throw new DisplayException("Email and password combination not found");
        }

        if (BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return user.TotpEnabled;
        }

        Logger.Warn($"Failed login attempt. Email: {email} Password: {password}", "security");
        throw new DisplayException("Email and password combination not found");;
    }

    public async Task<string> Login(string email, string password, string totpCode = "")
    {
        if (ConfigService.Get().Moonlight.Auth.DenyLogin)
            throw new DisplayException("This operation was disabled");
        
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
                //TODO: AuditLog
                return await GenerateToken(user, true);
            }
            else
            {
                Logger.Warn($"Failed login attempt. Email: {email} Password: {password}", "security");
                throw new DisplayException("2FA code invalid");
            }
        }
        else
        {
            //TODO: AuditLog
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
            //TODO: AuditLog
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

            //TODO: AuditLog
        }
    }

    public async Task<User> SftpLogin(int id, string password)
    {
        var user = UserRepository.Get().FirstOrDefault(x => x.Id == id);

        if (user == null)
        {
            Logger.Warn($"Detected an sftp bruteforce attempt. ID: {id} Password: {password}", "security");
            
            throw new Exception("Invalid username");
        }
        
        if (BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            //TODO: AuditLog
            return user;
        }
        
        Logger.Warn($"Detected an sftp bruteforce attempt. ID: {id} Password: {password}", "security");
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

        //TODO: AuditLog

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