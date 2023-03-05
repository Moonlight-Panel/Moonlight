using System.Diagnostics;
using System.Reflection;
using Moonlight.App.Database.Entities.LogsEntries;
using Moonlight.App.Repositories.LogEntries;
using Moonlight.App.Services.Sessions;
using Newtonsoft.Json;

namespace Moonlight.App.Services.LogServices;

public class ErrorLogService
{
    private readonly ErrorLogEntryRepository Repository;
    private readonly IdentityService IdentityService;
    
    public ErrorLogService(ErrorLogEntryRepository repository, IdentityService identityService)
    {
        Repository = repository;
        IdentityService = identityService;
    }
    
    public Task Log(Exception exception, params object[] objects)
    {
        var ip = IdentityService.GetIp();
        
        var entry = new ErrorLogEntry()
        {
            Ip = ip,
            System = false,
            JsonData = !objects.Any() ? "" : JsonConvert.SerializeObject(objects),
            Class = NameOfCallingClass(),
            Stacktrace = exception.ToStringDemystified()
        };

        Repository.Add(entry);
        
        return Task.CompletedTask;
    }
    
    public Task LogSystem(Exception exception, params object[] objects)
    {
        var entry = new ErrorLogEntry()
        {
            System = true,
            JsonData = !objects.Any() ? "" : JsonConvert.SerializeObject(objects),
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
}