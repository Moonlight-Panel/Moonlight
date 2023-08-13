using System.Diagnostics;
using System.Reflection;
using Moonlight.App.Database;
using Moonlight.App.Services;
using Moonlight.App.Services.Files;
using Serilog;

namespace Moonlight.App.Helpers;

public static class Logger
{
    // The private static instance of the config service, because we have no di here
    private static ConfigService ConfigService = new(new StorageService());
    
    #region String method calls
    public static void Verbose(string message, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Verbose("{Message}", message);
        
        if(channel == "security")
            LogSecurityInDb(message);
    }
    
    public static void Info(string message, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Information("{Message}", message);
        
        if(channel == "security")
            LogSecurityInDb(message);
    }
    
    public static void Debug(string message, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Debug("{Message}", message);
        
        if(channel == "security")
            LogSecurityInDb(message);
    }
    
    public static void Error(string message, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Error("{Message}", message);
        
        if(channel == "security")
            LogSecurityInDb(message);
    }
    
    public static void Warn(string message, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Warning("{Message}", message);
        
        if(channel == "security")
            LogSecurityInDb(message);
    }
    
    public static void Fatal(string message, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Fatal("{Message}", message);
        
        if(channel == "security")
            LogSecurityInDb(message);
    }
    #endregion
    
    #region Exception method calls
    public static void Verbose(Exception exception, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Verbose(exception, "");
        
        if(channel == "security")
            LogSecurityInDb(exception);
    }
    
    public static void Info(Exception exception, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Information(exception, "");
        
        if(channel == "security")
            LogSecurityInDb(exception);
    }
    
    public static void Debug(Exception exception, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Debug(exception, "");
        
        if(channel == "security")
            LogSecurityInDb(exception);
    }
    
    public static void Error(Exception exception, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Error(exception, "");
        
        if(channel == "security")
            LogSecurityInDb(exception);
    }
    
    public static void Warn(Exception exception, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Warning(exception, "");
        
        if(channel == "security")
            LogSecurityInDb(exception);
    }
    
    public static void Fatal(Exception exception, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Fatal(exception, "");
        
        if(channel == "security")
            LogSecurityInDb(exception);
    }
    #endregion
    
    private static string GetNameOfCallingClass(int skipFrames = 4)
    {
        string fullName;
        Type declaringType;
            
        do
        {
            MethodBase method = new StackFrame(skipFrames, false).GetMethod();
            declaringType = method.DeclaringType;
            if (declaringType == null)
            {
                return method.Name;
            }
            skipFrames++;
            if (declaringType.Name.Contains("<"))
                fullName = declaringType.ReflectedType.Name;
            else
                fullName = declaringType.Name;
        }
        while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase) | fullName.Contains("Logger"));

        return fullName;
    }


    private static void LogSecurityInDb(Exception exception)
    {
        LogSecurityInDb(exception.ToStringDemystified());
    }
    private static void LogSecurityInDb(string text)
    {
        Task.Run(() =>
        {
            var dataContext = new DataContext(ConfigService);
            
            dataContext.SecurityLogs.Add(new()
            {
                Text = text
            });

            dataContext.SaveChanges();
            dataContext.Dispose();
        });
    }
}