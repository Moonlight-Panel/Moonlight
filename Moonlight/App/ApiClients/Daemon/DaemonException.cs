using System.Runtime.Serialization;

namespace Moonlight.App.ApiClients.Daemon;

[Serializable]
public class DaemonException : Exception
{
    public int StatusCode { get; set; }

    public DaemonException()
    {
    }

    public DaemonException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public DaemonException(string message) : base(message)
    {
    }

    public DaemonException(string message, Exception inner) : base(message, inner)
    {
    }

    protected DaemonException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}