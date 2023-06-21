using System.Diagnostics;
using System.Reflection;
using Serilog;

namespace Moonlight.App.Helpers;

public static class Logger
{
    #region String method calls
    public static void Verbose(string message, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Verbose("{Message} {Channel}", message, channel);
    }
    
    public static void Info(string message, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Information("{Message} {Channel}", message, channel);
    }
    
    public static void Debug(string message, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Debug("{Message} {Channel}", message, channel);
    }
    
    public static void Error(string message, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Error("{Message} {Channel}", message, channel);
    }
    
    public static void Warn(string message, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Warning("{Message} {Channel}", message, channel);
    }
    
    public static void Fatal(string message, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Fatal("{Message} {Channel}", message, channel);
    }
    #endregion
    
    #region Exception method calls
    public static void Verbose(Exception exception, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Verbose(exception, "{Channel}", channel);
    }
    
    public static void Info(Exception exception, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Information(exception, "{Channel}", channel);
    }
    
    public static void Debug(Exception exception, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Debug(exception, "{Channel}", channel);
    }
    
    public static void Error(Exception exception, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Error(exception, "{Channel}", channel);
    }
    
    public static void Warn(Exception exception, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Warning(exception, "{Channel}", channel);
    }
    
    public static void Fatal(Exception exception, string channel = "default")
    {
        Log.ForContext("SourceContext", GetNameOfCallingClass())
            .Fatal(exception, "{Channel}", channel);
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
}