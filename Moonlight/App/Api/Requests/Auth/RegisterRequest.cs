using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Moonlight.App.Database.Entities;
using Moonlight.App.Event;
using Moonlight.App.Exceptions;
using Moonlight.App.Extensions;
using Moonlight.App.Helpers;
using Moonlight.App.Repositories;
using Moonlight.App.Services;
using Moonlight.App.Services.Utils;

namespace Moonlight.App.Api.Requests.Auth;

[ApiRequest(4)]
public class RegisterRequest: AbstractRequest
{
    [Required(ErrorMessage = "9")]
    [EmailAddress(ErrorMessage = "10")]
    public string Email { get; set; } = "";
    
    [Required(ErrorMessage = "8")]
    [MinLength(7, ErrorMessage = "7")]
    [MaxLength(20, ErrorMessage = "7")]
    [RegularExpression("^[a-z][a-z0-9]*$", ErrorMessage = "6")]
    public string Username { get; set; } = "";
    
    [Required(ErrorMessage = "4")]
    [MinLength(8, ErrorMessage = "5")]
    [MaxLength(256, ErrorMessage = "5")]
    public string Password { get; set; } = "";
    public string PasswordConfirm { get; set; } = "";
    
    public bool Success { get; set; }
    public bool RequireEmailVerify { get; set; }
    /// <summary>
    /// Error Codes:
    /// - 0 all successful
    /// - 1 email exists
    /// - 2 username exists
    /// - 3 passwords do not match
    /// - 4 password needs to be provided
    /// - 5 password needs to be between 8 and 256 characters
    /// - 6 Usernames can only contain lowercase characters and numbers
    /// - 7 username has to be between 7 and 20 chars
    /// - 8 username required
    /// - 9 email required
    /// - 10 email invalid
    /// </summary>
    public int ErrorCode { get; set; }
    public string Token { get; set; } = "";
    public override ResponseDataBuilder CreateResponse(ResponseDataBuilder builder)
    {
        builder.WriteBoolean(Success);
        builder.WriteBoolean(RequireEmailVerify);
        builder.WriteInt(ErrorCode);
        builder.WriteString(Token);

        return builder;
    }

    public override void ReadData(RequestDataContext dataContext)
    {
        Email = dataContext.ReadString();
        Username = dataContext.ReadString();
        Password = dataContext.ReadString();
        PasswordConfirm = dataContext.ReadString();
    }

    public override async Task ProcessRequest()
    {
        var userRepository = ServiceProvider.GetRequiredService<Repository<User>>();
        var configService = ServiceProvider.GetRequiredService<ConfigService>();

        var reqEmailVerify = configService.Get().Security.EnableEmailVerify;
        // Event though we have form validation i want to
        // ensure that at least these basic formatting things are done
        Email = Email.ToLower().Trim();
        Username = Username.ToLower().Trim();

        if (PasswordConfirm != Password)
        {
            Token = "";
            Success = false;
            RequireEmailVerify = false;
            ErrorCode = 3;
            return;
        }

        if (!IsPropertyValid(this.GetProperty(x => x.Email)!, out var errorCd))
        {
            Token = "";
            Success = false;
            RequireEmailVerify = false;
            ErrorCode = errorCd;
            return;
        }

        if (!IsPropertyValid(this.GetProperty(x => x.Password)!, out var errorCd1))
        {
            Token = "";
            Success = false;
            RequireEmailVerify = false;
            ErrorCode = errorCd1;
            return;
        }

        if (!IsPropertyValid(this.GetProperty(x => x.Username)!, out var errorCd2))
        {
            Token = "";
            Success = false;
            RequireEmailVerify = false;
            ErrorCode = errorCd2;
            return;
        }

        // Prevent duplication or username and/or email
        if (userRepository.Get().Any(x => x.Email == Email))
        {            
            Token = "";
            Success = false;
            RequireEmailVerify = false;
            ErrorCode = 1;
            return;
        }

        if (userRepository.Get().Any(x => x.Username == Username))
        {
            Token = "";
            Success = false;
            RequireEmailVerify = false;
            ErrorCode = 2;
            return;
        }

        var user = new User()
        {
            Username = Username,
            Email = Email,
            Password = HashHelper.HashToString(Password)
        };

        var result = userRepository.Add(user);

        await Events.OnUserRegistered.InvokeAsync(result);

        Token = await GenerateToken(user);
        Success = true;
        RequireEmailVerify = reqEmailVerify;
        ErrorCode = 0;
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

    private bool IsPropertyValid(PropertyInfo property, out int errorCode)
    {
        var attribs = property.GetCustomAttributes<ValidationAttribute>();

        foreach (var a in attribs)
        {
            if (!a.IsValid(property.GetValue(this)))
            {
                errorCode = int.Parse(a.ErrorMessage);
                return false;
            }
        }

        errorCode = 0;
        return true;
    }
}