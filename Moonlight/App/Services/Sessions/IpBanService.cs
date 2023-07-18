using Moonlight.App.Database.Entities;
using Moonlight.App.Events;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services.Sessions;

public class IpBanService
{
    private readonly IdentityService IdentityService;
    private readonly Repository<IpBan> IpBanRepository;
    
    public IpBanService(
        IdentityService identityService,
        Repository<IpBan> ipBanRepository)
    {
        IdentityService = identityService;
        IpBanRepository = ipBanRepository;
    }

    public Task<bool> IsBanned()
    {
        var ip = IdentityService.Ip;

        return Task.FromResult(
            IpBanRepository
                .Get()
                .Any(x => x.Ip == ip)
        );
    }
}