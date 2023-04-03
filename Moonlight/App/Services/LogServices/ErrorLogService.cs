using System.Diagnostics;
using System.Reflection;
using Moonlight.App.Database.Entities.LogsEntries;
using Moonlight.App.Models.Log;
using Moonlight.App.Repositories.LogEntries;
using Moonlight.App.Services.Sessions;
using Newtonsoft.Json;

namespace Moonlight.App.Services.LogServices;

public class ErrorLogService
{
    private readonly ErrorLogEntryRepository Repository;
    private readonly IHttpContextAccessor HttpContextAccessor;
    
    public ErrorLogService(ErrorLogEntryRepository repository, IHttpContextAccessor httpContextAccessor)
    {
        Repository = repository;
        HttpContextAccessor = httpContextAccessor;
    }
    
    public Task Log(Exception exception, Action<ErrorLogParameters> data)
    {
        var ip = GetIp();
        var al = new ErrorLogParameters();
        data(al);
        
        var entry = new ErrorLogEntry()
        {
            Ip = ip,
            System = false,
            JsonData = al.Build(),
            Class = NameOfCallingClass(),
            Stacktrace = exception.ToStringDemystified()
        };

        Repository.Add(entry);
        
        return Task.CompletedTask;
    }
    
    public Task LogSystem(Exception exception, Action<ErrorLogParameters> data)
    {
        var al = new ErrorLogParameters();
        data(al);
        
        var entry = new ErrorLogEntry()
        {
            System = true,
            JsonData = al.Build(),
            Class = NameOfCallingClass(),
            Stacktrace = exception.ToStringDemystified()
        };

        Repository.Add(entry);
        
        return Task.CompletedTask;
    }
    
    private string NameOfCallingClass(int skipFrames = 4)
    {
        string fullName;
        Type? declaringType;
            
        do
        {
            MethodBase method = new StackFrame(skipFrames, false).GetMethod()!;
            declaringType = method.DeclaringType;
            if (declaringType == null)
            {
                return method.Name;
            }
            skipFrames++;
            if (declaringType.Name.Contains("<"))
                fullName = declaringType.ReflectedType!.Name;
            else
                fullName = declaringType.Name;
        }
        while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase) | fullName.Contains("Log"));

        return fullName;
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
    
    public class ErrorLogParameters
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