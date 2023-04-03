using Moonlight.App.Database.Entities.LogsEntries;
using Moonlight.App.Models.Log;
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

    public Task Log(AuditLogType type, Action<AuditLogParameters> data)
    {
        var ip = GetIp();
        var al = new AuditLogParameters();
        data(al);
        
        var entry = new AuditLogEntry()
        {
            Ip = ip,
            Type = type,
            System = false,
            JsonData = al.Build()
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
    
    public class AuditLogParameters
    {
        private List<LogData> Data = new List<LogData>();

        public void Add<T>(object data)
        {
            Data.Add(new LogData()
            {
                Type = typeof(T),
                Value = data.ToString()
            });
        }

        internal string Build()
        {
            return JsonConvert.SerializeObject(Data);
        }
    }
}