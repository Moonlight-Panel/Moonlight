using Moonlight.App.Database.Entities.LogsEntries;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories.LogEntries;
using Moonlight.App.Services.Sessions;
using Newtonsoft.Json;

namespace Moonlight.App.Services.LogServices;

public class AuditLogService
{
    private readonly AuditLogEntryRepository Repository;
    private readonly IdentityService IdentityService;

    public AuditLogService(AuditLogEntryRepository repository, IdentityService identityService)
    {
        Repository = repository;
        IdentityService = identityService;
    }

    public Task Log(AuditLogType type, object? data = null)
    {
        var ip = IdentityService.GetIp();
        
        var entry = new AuditLogEntry()
        {
            Ip = ip,
            Type = type,
            System = false,
            JsonData = data == null ? "" : JsonConvert.SerializeObject(data)
        };

        Repository.Add(entry);
        
        return Task.CompletedTask;
    }
    
    public Task LogSystem(AuditLogType type, object? data = null)
    {
        var entry = new AuditLogEntry()
        {
            Type = type,
            System = true,
            JsonData = data == null ? "" : JsonConvert.SerializeObject(data)
        };

        Repository.Add(entry);
        
        return Task.CompletedTask;
    }
}