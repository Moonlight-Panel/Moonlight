using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Abstractions;
using Moonlight.App.Models.Enums;
using Moonlight.App.Repositories;
using Moonlight.App.Services.Utils;
using OtpNet;

namespace Moonlight.App.Api.Requests.Auth;

[ApiRequest(3)]
public class CredentialBasedLoginRequest : AbstractRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Code { get; set; }
    
    public bool Success { get; set; }
    public bool RequireTotp { get; set; }
    /// <summary>
    /// 0: all fine
    /// 1: wrong credentials
    /// 2: Totp enabled
    /// 3: TotpKey missing
    /// 4: wrong totp code
    /// </summary>
    public int ErrorId { get; set; }
    public string Token { get; set; } = "";
    public override ResponseDataBuilder CreateResponse(ResponseDataBuilder builder)
    {
        builder.WriteBoolean(Success);
        builder.WriteBoolean(RequireTotp);
        builder.WriteInt(ErrorId);
        builder.WriteString(Token);

        return builder;
    }

    public override async Task ProcessRequest()
    {
        var userRepository = ServiceProvider.GetService<Repository<User>>();
        
        var user = userRepository
            .Get()
            .FirstOrDefault(x => x.Email == Email);

        if (user == null)
        {
            Success = false;
            RequireTotp = false;
            ErrorId = 1;
            Token = "";
            return;
        }

        if (!HashHelper.Verify(Password, user.Password))
        {
            Success = false;
            RequireTotp = false;
            ErrorId = 1;
            Token = "";
            return;
        }

        var flags = new FlagStorage(user.Flags); // Construct FlagStorage to check for 2fa

        if (!flags[UserFlag.TotpEnabled])
        {
            // No 2fa found on this user so were done here
            Success = true;
            RequireTotp = false;
            ErrorId = 0;
            Token = await GenerateToken(user);
            Context!.User = user;
            return;
        }

        // If we reach this point, 2fa is enabled so we need to continue validating

        if (string.IsNullOrEmpty(Code))
        {
            // This will show an additional 2fa login field
            Success = false;
            RequireTotp = true;
            ErrorId = 2;
            Token = "";
            return;
        }

        if (user.TotpKey == null)
        {
            // Hopefully we will never fulfill this check ;)
            Success = false;
            RequireTotp = false;
            ErrorId = 3;
            Token = "";
            return;
            throw new DisplayException("2FA key is missing. Please contact the support to fix your account");
        }

        // Calculate server side code
        var totp = new Totp(Base32Encoding.ToBytes(user.TotpKey));
        var codeServerSide = totp.ComputeTotp();

        if (codeServerSide == Code)
        {
            Success = true;
            RequireTotp = false;
            ErrorId = 0;
            Token = await GenerateToken(user);
            Context!.User = user;
            return;
        }

        Success = false;
        RequireTotp = false;
        ErrorId = 4;
        Token = "";
    }
    
    public async Task<string> GenerateToken(User user)
    {
        var jwtService = ServiceProvider.GetService<JwtService>();
        
        var token = await jwtService.Create(data =>
        {
            data.Add("userId", user.Id.ToString());
            data.Add("issuedAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        }, TimeSpan.FromDays(365));

        return token;
    }

    public override void ReadData(RequestDataContext dataContext)
    {
        Email = dataContext.ReadString();
        Password = dataContext.ReadString();
        Code = dataContext.ReadString();
    }
}