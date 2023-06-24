using CloudFlare.Client;
using CloudFlare.Client.Api.Authentication;
using CloudFlare.Client.Api.Display;
using CloudFlare.Client.Api.Parameters.Data;
using CloudFlare.Client.Api.Result;
using CloudFlare.Client.Api.Zones;
using CloudFlare.Client.Api.Zones.DnsRecord;
using CloudFlare.Client.Enumerators;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories.Domains;
using DnsRecord = Moonlight.App.Models.Misc.DnsRecord;
using MatchType = CloudFlare.Client.Enumerators.MatchType;

namespace Moonlight.App.Services;

public class DomainService
{
    private readonly DomainRepository DomainRepository;
    private readonly SharedDomainRepository SharedDomainRepository;
    private readonly CloudFlareClient Client;
    private readonly string AccountId;

    public DomainService(
        ConfigService configService,
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

    public Task<Domain> Create(string domain, SharedDomain sharedDomain, User user)
    {
        if (DomainRepository.Get().Where(x => x.SharedDomain.Id == sharedDomain.Id).Any(x => x.Name == domain))
            throw new DisplayException("A domain with this name does already exist for this shared domain");

        var res = DomainRepository.Add(new()
        {
            Name = domain,
            SharedDomain = sharedDomain,
            Owner = user
        });

        return Task.FromResult(res);
    }

    public Task Delete(Domain domain)
    {
        DomainRepository.Delete(domain);

        return Task.CompletedTask;
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

        var records = new List<CloudFlare.Client.Api.Zones.DnsRecord.DnsRecord>();

        // Load paginated
        // TODO: Find an alternative option. This way to load the records is NOT optimal
        // and can result in long loading time when there are many dns records.
        // As cloudflare does not offer a way to search dns records which starts
        // with a specific string we are not able to filter it using the api (client)
        var initialResponse = await Client.Zones.DnsRecords.GetAsync(domain.SharedDomain.CloudflareId);

        records.AddRange(GetData(initialResponse));

        // Check if there are more pages
        while (initialResponse.ResultInfo.Page < initialResponse.ResultInfo.TotalPage)
        {
            // Get the next page of data
            var nextPageResponse = await Client.Zones.DnsRecords.GetAsync(
                domain.SharedDomain.CloudflareId,
                displayOptions: new()
                {
                    Page = initialResponse.ResultInfo.Page + 1
                }
            );
            var nextPageRecords = GetData(nextPageResponse);

            // Append the records from the next page to the existing records
            records.AddRange(nextPageRecords);

            // Update the initial response to the next page response
            initialResponse = nextPageResponse;
        }

        var rname = $"{domain.Name}.{domain.SharedDomain.Name}";
        var dname = $".{rname}";

        var result = new List<DnsRecord>();

        foreach (var record in records)
        {
            if (record.Name.EndsWith(dname))
            {
                result.Add(new()
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
                result.Add(new()
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

    public async Task AddDnsRecord(Domain d, DnsRecord dnsRecord)
    {
        var domain = EnsureData(d);

        var rname = $"{domain.Name}.{domain.SharedDomain.Name}";
        var dname = $".{rname}";

        if (dnsRecord.Type == DnsRecordType.Srv)
        {
            var parts = dnsRecord.Name.Split(".");

            Protocol protocol = Protocol.Tcp;

            if (parts[1].Contains("udp"))
                protocol = Protocol.Udp;

            var valueParts = dnsRecord.Content.Split(" ");

            var nameWithoutProt = dnsRecord.Name.Replace($"{parts[0]}.{parts[1]}.", "");
            nameWithoutProt = nameWithoutProt.Replace($"{parts[0]}.{parts[1]}", "");
            var name = nameWithoutProt == "" ? rname : nameWithoutProt + dname;

            var srv = new NewDnsRecord<Srv>()
            {
                Type = dnsRecord.Type,
                Data = new()
                {
                    Service = parts[0],
                    Protocol = protocol,
                    Name = name,
                    Weight = int.Parse(valueParts[0]),
                    Port = int.Parse(valueParts[1]),
                    Target = valueParts[2],
                    Priority = dnsRecord.Priority
                },
                Proxied = dnsRecord.Proxied,
                Ttl = dnsRecord.Ttl,
            };

            GetData(await Client.Zones.DnsRecords.AddAsync(d.SharedDomain.CloudflareId, srv));
        }
        else
        {
            var name = string.IsNullOrEmpty(dnsRecord.Name) ? rname : dnsRecord.Name + dname;

            GetData(await Client.Zones.DnsRecords.AddAsync(d.SharedDomain.CloudflareId, new NewDnsRecord()
            {
                Type = dnsRecord.Type,
                Priority = dnsRecord.Priority,
                Content = dnsRecord.Content,
                Proxied = dnsRecord.Proxied,
                Ttl = dnsRecord.Ttl,
                Name = name
            }));
        }

        //TODO: AuditLog
    }

    public async Task UpdateDnsRecord(Domain d, DnsRecord dnsRecord)
    {
        var domain = EnsureData(d);

        var rname = $"{domain.Name}.{domain.SharedDomain.Name}";
        var dname = $".{rname}";

        if (dnsRecord.Type == DnsRecordType.Srv)
        {
            throw new DisplayException(
                "SRV records cannot be updated thanks to the cloudflare api client. Please delete the record and create a new one");
        }
        else
        {
            var name = dnsRecord.Name == "" ? rname : dnsRecord.Name + dname;

            GetData(await Client.Zones.DnsRecords.UpdateAsync(d.SharedDomain.CloudflareId, dnsRecord.Id,
                new ModifiedDnsRecord()
                {
                    Content = dnsRecord.Content,
                    Proxied = dnsRecord.Proxied,
                    Ttl = dnsRecord.Ttl,
                    Name = name,
                    Type = dnsRecord.Type
                }));
        }

        //TODO: AuditLog
    }

    public async Task DeleteDnsRecord(Domain d, DnsRecord dnsRecord)
    {
        var domain = EnsureData(d);

        GetData(
            await Client.Zones.DnsRecords.DeleteAsync(domain.SharedDomain.CloudflareId, dnsRecord.Id)
        );

        //TODO: AuditLog
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
            string message;

            try
            {
                message = result.Errors.First().ErrorChain.First().Message;
            }
            catch (Exception)
            {
                throw new CloudflareException("No error message provided");
            }

            throw new CloudflareException(message);
        }

        return result.Result;
    }
}