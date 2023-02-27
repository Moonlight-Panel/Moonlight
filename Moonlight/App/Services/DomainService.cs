using CloudFlare.Client;
using CloudFlare.Client.Api.Authentication;
using CloudFlare.Client.Api.Result;
using CloudFlare.Client.Api.Zones;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Models.Misc;
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
        var domains = GetData(
                await Client.Zones.GetAsync(new()
                {
                    AccountId = AccountId
                })
        );

        var sharedDomains = SharedDomainRepository.Get().ToArray();

        var freeDomains = domains
            .Where(x => sharedDomains.FirstOrDefault
                (
                    y => y.CloudflareId == x.Id
                ) == null
            )
            .ToArray();

        return freeDomains;
    }

    public async Task<DnsRecord[]> GetDnsRecords(Domain d)
    {
        var domain = EnsureData(d);

        var records = GetData(
            await Client.Zones.DnsRecords.GetAsync(domain.SharedDomain.CloudflareId)
        );

        var rname = $"{domain.Name}.{domain.SharedDomain.Name}";
        var dname = $".{rname}";

        var result = new List<DnsRecord>();

        foreach (var record in records)
        {
            if (record.Name.EndsWith(dname))
            {
                result.Add(new ()
                {
                    Name = record.Name.Replace(dname, ""),
                    Content = record.Content,
                    Priority = record.Priority ?? 0,
                    Proxied = record.Proxied ?? false,
                    Id = record.Id,
                    Ttl = record.Ttl ?? 0,
                    Type = record.Type
                });
            }
            else if (record.Name.EndsWith(rname))
            {
                result.Add(new ()
                {
                    Name = record.Name.Replace(rname, ""),
                    Content = record.Content,
                    Priority = record.Priority ?? 0,
                    Proxied = record.Proxied ?? false,
                    Id = record.Id,
                    Ttl = record.Ttl ?? 0,
                    Type = record.Type
                });
            }
        }
        
        return result.ToArray();
    }

    private Domain EnsureData(Domain domain)
    {
        if (domain.SharedDomain != null)
            return domain;
        else
            return DomainRepository
                .Get()
                .Include(x => x.SharedDomain)
                .First(x => x.Id == domain.Id);
    }
    private T GetData<T>(CloudFlareResult<T> result)
    {
        if (!result.Success)
        {
            var message = result.Errors.First().ErrorChain.First().Message;

            throw new CloudflareException(message);
        }

        return result.Result;
    }
}