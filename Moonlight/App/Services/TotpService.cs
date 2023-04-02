using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories;
using Moonlight.App.Services.LogServices;
using Moonlight.App.Services.Sessions;
using OtpNet;

namespace Moonlight.App.Services;

public class TotpService
{
    private readonly IdentityService IdentityService;
    private readonly UserRepository UserRepository;
    private readonly AuditLogService AuditLogService;

    public TotpService(
        IdentityService identityService, 
        UserRepository userRepository, 
        AuditLogService auditLogService)
    {
        IdentityService = identityService;
        UserRepository = userRepository;
        AuditLogService = auditLogService;
    }

    public Task<bool> Verify(string secret, string code)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secret));
        var codeserver = totp.ComputeTotp();
        return Task.FromResult(codeserver == code);
    }

    public async Task<bool> GetEnabled()
    {
        var user = await IdentityService.Get();

        return user!.TotpEnabled;
    }

    public async Task<string> GetSecret()
    {
        var user = await IdentityService.Get();
        
        return user!.TotpSecret;
    }

    public async Task Enable()
    {
        var user = (await IdentityService.Get())!;
        
        user.TotpSecret = GenerateSecret();
        
        UserRepository.Update(user);

        await AuditLogService.Log(AuditLogType.EnableTotp, user.Email);
    }

    public async Task EnforceTotpLogin()
    {
        var user = (await IdentityService.Get())!;

        user.TotpEnabled = true;
        UserRepository.Update(user);
    }

    public async Task Disable()
    {
        var user = (await IdentityService.Get())!;

        user.TotpEnabled = false;

        UserRepository.Update(user);
        
        await AuditLogService.Log(AuditLogType.DisableTotp, user.Email);
    }

    private string GenerateSecret()
    {
        return Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
    }
}