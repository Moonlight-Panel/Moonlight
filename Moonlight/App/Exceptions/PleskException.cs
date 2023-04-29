using System.Runtime.Serialization;

namespace Moonlight.App.Exceptions;

[Serializable]
public class PleskException : Exception
{
    public int StatusCode { private get; set; }

    public PleskException()
    {
    }

    public PleskException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public PleskException(string message) : base(message)
    {
    }

    public PleskException(string message, Exception inner) : base(message, inner)
    {
    }

    protected PleskException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}