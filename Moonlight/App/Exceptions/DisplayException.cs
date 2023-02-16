using System.Runtime.Serialization;

namespace Moonlight.App.Exceptions;

[Serializable]
public class DisplayException : Exception
{
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public DisplayException()
    {
    }

    public DisplayException(string message) : base(message)
    {
    }

    public DisplayException(string message, Exception inner) : base(message, inner)
    {
    }

    protected DisplayException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}