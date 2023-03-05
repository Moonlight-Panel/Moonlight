using Moonlight.App.Database.Entities.LogsEntries;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories.LogEntries;
using Moonlight.App.Services.Sessions;
using Newtonsoft.Json;

namespace Moonlight.App.Services.LogServices;

public class SecurityLogService
{
    private readonly SecurityLogEntryRepository Repository;
    private readonly IdentityService IdentityService;

    public SecurityLogService(SecurityLogEntryRepository repository, IdentityService identityService)
    {
        Repository = repository;
        IdentityService = identityService;
    }

    public Task Log(SecurityLogType type, object? data = null)
    {
        var ip = IdentityService.GetIp();
        
        var entry = new SecurityLogEntry()
        {
            Ip = ip,
            Type = type,
            System = false,
            JsonData = data == null ? "" : JsonConvert.SerializeObject(data)
        };

        Repository.Add(entry);
        
        return Task.CompletedTask;
    }
    
    public Task LogSystem(SecurityLogType type, object? data = null)
    {
        var entry = new SecurityLogEntry()
        {
            Type = type,
            System = true,
            JsonData = data == null ? "" : JsonConvert.SerializeObject(data)
        };

        Repository.Add(entry);
        
        return Task.CompletedTask;
    }
}