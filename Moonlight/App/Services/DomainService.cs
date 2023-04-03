using CloudFlare.Client;
using CloudFlare.Client.Api.Authentication;
using CloudFlare.Client.Api.Parameters.Data;
using CloudFlare.Client.Api.Result;
using CloudFlare.Client.Api.Zones;
using CloudFlare.Client.Api.Zones.DnsRecord;
using CloudFlare.Client.Enumerators;
using Logging.Net;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories.Domains;
using Moonlight.App.Services.LogServices;
using DnsRecord = Moonlight.App.Models.Misc.DnsRecord;

namespace Moonlight.App.Services;

public class DomainService
{
    private readonly DomainRepository DomainRepository;
    private readonly SharedDomainRepository SharedDomainRepository;
    private readonly CloudFlareClient Client;
    private readonly AuditLogService AuditLogService;
    private readonly string AccountId;

    public DomainService(
        ConfigService configService,
        DomainRepository domainRepository,
        SharedDomainRepository sharedDomainRepository,
        AuditLogService auditLogService)
    {
        DomainRepository = domainRepository;
        SharedDomainRepository = sharedDomainRepository;
        AuditLogService = auditLogService;

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
            Enum.TryParse(parts[1], out Protocol protocol);

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

        await AuditLogService.Log(AuditLogType.AddDomainRecord, x =>
        {
            x.Add<Domain>(d.Id);
            x.Add<DnsRecord>(dnsRecord.Name);
        });
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
        
        await AuditLogService.Log(AuditLogType.UpdateDomainRecord, x =>
        {
            x.Add<Domain>(d.Id);
            x.Add<DnsRecord>(dnsRecord.Name);
        });
    }

    public async Task DeleteDnsRecord(Domain d, DnsRecord dnsRecord)
    {
        var domain = EnsureData(d);

        GetData(
            await Client.Zones.DnsRecords.DeleteAsync(domain.SharedDomain.CloudflareId, dnsRecord.Id)
        );
        
        await AuditLogService.Log(AuditLogType.DeleteDomainRecord, x =>
        {
            x.Add<Domain>(d.Id);
            x.Add<DnsRecord>(dnsRecord.Name);
        });
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