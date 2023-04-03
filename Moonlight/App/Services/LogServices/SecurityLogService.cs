using Moonlight.App.Database.Entities.LogsEntries;
using Moonlight.App.Models.Log;
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

    public Task Log(SecurityLogType type, Action<SecurityLogParameters> data)
    {
        var ip = GetIp();
        var al = new SecurityLogParameters();
        data(al);
        
        var entry = new SecurityLogEntry()
        {
            Ip = ip,
            Type = type,
            System = false,
            JsonData = al.Build()
        };

        Repository.Add(entry);
        
        return Task.CompletedTask;
    }
    
    public Task LogSystem(SecurityLogType type, Action<SecurityLogParameters> data)
    {
        var al = new SecurityLogParameters();
        data(al);
        
        var entry = new SecurityLogEntry()
        {
            Type = type,
            System = true,
            JsonData = al.Build()
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
    
    
    public class SecurityLogParameters
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