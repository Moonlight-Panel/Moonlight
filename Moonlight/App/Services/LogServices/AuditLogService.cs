using Moonlight.App.Database.Entities.LogsEntries;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories.LogEntries;
using Moonlight.App.Services.Sessions;
using Newtonsoft.Json;

namespace Moonlight.App.Services.LogServices;

public class AuditLogService
{
    private readonly AuditLogEntryRepository Repository;
    private readonly IHttpContextAccessor HttpContextAccessor;

    public AuditLogService(
        AuditLogEntryRepository repository,
        IHttpContextAccessor httpContextAccessor)
    {
        Repository = repository;
        HttpContextAccessor = httpContextAccessor;
    }

    public Task Log(AuditLogType type, params object[] data)
    {
        var ip = GetIp();
        
        var entry = new AuditLogEntry()
        {
            Ip = ip,
            Type = type,
            System = false,
            JsonData = data.Length == 0 ? "" : JsonConvert.SerializeObject(data)
        };

        Repository.Add(entry);
        
        return Task.CompletedTask;
    }
    
    public Task LogSystem(AuditLogType type, params object[] data)
    {
        var entry = new AuditLogEntry()
        {
            Type = type,
            System = true,
            JsonData = data.Length == 0 ? "" : JsonConvert.SerializeObject(data)
        };

        Repository.Add(entry);
        
        return Task.CompletedTask;
    }
    
    private string GetIp()
    {
        if (HttpContextAccessor.HttpContext == null)
            return "N/A";

        if(HttpContextAccessor.HttpContext.Request.Headers.ContainsKey("X-Real-IP"))
        {
            return HttpContextAccessor.HttpContext.Request.Headers["X-Real-IP"]!;
        }
        
        return HttpContextAccessor.HttpContext.Connection.RemoteIpAddress!.ToString();
    }
}