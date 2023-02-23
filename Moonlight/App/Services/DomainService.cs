using CloudFlare.Client;
using CloudFlare.Client.Api.Authentication;
using CloudFlare.Client.Api.Result;
using CloudFlare.Client.Api.Zones;
using Moonlight.App.Repositories.Domains;

namespace Moonlight.App.Services;

public class DomainService
{
    private readonly DomainRepository DomainRepository;
    private readonly SharedDomainRepository SharedDomainRepository;
    private readonly CloudFlareClient Client;
    private readonly string AccountId;

    public DomainService(ConfigService configService,
        DomainRepository domainRepository,
        SharedDomainRepository sharedDomainRepository)
    {
        DomainRepository = domainRepository;
        SharedDomainRepository = sharedDomainRepository;

        var config = configService
            .GetSection("Moonlight")
            .GetSection("Domains");

        AccountId = config.GetValue<string>("AccountId");

        Client = new(
            new ApiKeyAuthentication(
                config.GetValue<string>("Email"),
                config.GetValue<string>("Key")
            )
        );
    }

    public async Task<Zone[]>
        GetAvailableDomains() // This method returns all available domains which are not added as a shared domain
    {
        var domains = await Client.Zones.GetAsync(new()
        {
            AccountId = AccountId
        });

        if (!domains.Success)
            throw new CloudflareException(GetErrorMessage(domains));

        var sharedDomains = SharedDomainRepository.Get().ToArray();

        var freeDomains = domains.Result
            .Where(x => sharedDomains.FirstOrDefault
                (
                    y => y.CloudflareId == x.Id
                ) == null
            )
            .ToArray();

        return freeDomains;
    }

    private string GetErrorMessage<T>(CloudFlareResult<T> result)
    {
        return result.Errors.First().ErrorChain.First().Message;
    }
}