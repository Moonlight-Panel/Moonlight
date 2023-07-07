using System.Runtime.Serialization;

namespace Moonlight.App.ApiClients.Telemetry;

[Serializable]
public class TelemetryException : Exception
{
    public int StatusCode { get; set; }

    public TelemetryException()
    {
    }
    
    public TelemetryException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public TelemetryException(string message) : base(message)
    {
    }

    public TelemetryException(string message, Exception inner) : base(message, inner)
    {
    }

    protected TelemetryException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}