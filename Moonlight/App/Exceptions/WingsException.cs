using System.Runtime.Serialization;

namespace Moonlight.App.Exceptions;

[Serializable]
public class WingsException : Exception
{
    public int StatusCode { get; set; }

    public WingsException()
    {
    }
    
    public WingsException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public WingsException(string message) : base(message)
    {
    }

    public WingsException(string message, Exception inner) : base(message, inner)
    {
    }

    protected WingsException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}