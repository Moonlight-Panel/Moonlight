using System.Runtime.Serialization;

namespace Moonlight.App.ApiClients.CloudPanel;

[Serializable]
public class CloudPanelException : Exception
{
    public int StatusCode { get; set; }

    public CloudPanelException()
    {
    }
    
    public CloudPanelException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public CloudPanelException(string message) : base(message)
    {
    }

    public CloudPanelException(string message, Exception inner) : base(message, inner)
    {
    }

    protected CloudPanelException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}