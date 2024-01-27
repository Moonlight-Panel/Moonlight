namespace Moonlight.Features.Servers.Exceptions;

public class NodeException : Exception
{
    public NodeException()
    {
    }

    public NodeException(string message) : base(message)
    {
    }

    public NodeException(string message, Exception inner) : base(message, inner)
    {
    }
}