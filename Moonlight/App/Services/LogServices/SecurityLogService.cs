using Moonlight.App.Database.Entities.LogsEntries;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories.LogEntries;
using Moonlight.App.Services.Sessions;
using Newtonsoft.Json;

namespace Moonlight.App.Services.LogServices;

public class SecurityLogService
{
    private readonly SecurityLogEntryRepository Repository;
    private readonly IHttpContextAccessor HttpContextAccessor;

    public SecurityLogService(SecurityLogEntryRepository repository, IHttpContextAccessor httpContextAccessor)
    {
        Repository = repository;
        HttpContextAccessor = httpContextAccessor;
    }

    public Task Log(SecurityLogType type, params object[] data)
    {
        var ip = GetIp();
        
        var entry = new SecurityLogEntry()
        {
            Ip = ip,
            Type = type,
            System = false,
            JsonData = data.Length == 0 ? "" : JsonConvert.SerializeObject(data)
        };

        Repository.Add(entry);
        
        return Task.CompletedTask;
    }
    
    public Task LogSystem(SecurityLogType type, params object[] data)
    {
        var entry = new SecurityLogEntry()
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