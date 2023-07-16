using Moonlight.App.Exceptions;
using Moonlight.App.Repositories;
using Moonlight.App.Services.Sessions;
using OtpNet;

namespace Moonlight.App.Services;

public class TotpService
{
    private readonly IdentityService IdentityService;
    private readonly UserRepository UserRepository;

    public TotpService(
        IdentityService identityService, 
        UserRepository userRepository)
    {
        IdentityService = identityService;
        UserRepository = userRepository;
    }

    public Task<bool> Verify(string secret, string code)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secret));
        var codeserver = totp.ComputeTotp();
        return Task.FromResult(codeserver == code);
    }

    public Task<bool> GetEnabled()
    {
        return Task.FromResult(IdentityService.User.TotpEnabled);
    }

    public Task<string> GetSecret()
    {
        return Task.FromResult(IdentityService.User.TotpSecret);
    }

    public Task GenerateSecret()
    {
        var user = IdentityService.User;
        
        user.TotpSecret = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));;
        
        UserRepository.Update(user);
        
        return Task.CompletedTask;
    }

    public async Task Enable(string code)
    {
        var user = IdentityService.User;

        if (!await Verify(user.TotpSecret, code))
        {
            throw new DisplayException("The 2fa code you entered is invalid");
        }

        user.TotpEnabled = true;
        UserRepository.Update(user);
    }

    public Task Disable()
    {
        var user = IdentityService.User;

        user.TotpEnabled = false;
        user.TotpSecret = "";

        UserRepository.Update(user);
        
        //TODO: AuditLog
        
        return Task.CompletedTask;
    }
}