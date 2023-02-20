using Moonlight.App.MessageSystem;

namespace Moonlight.App.Services;

public class MessageService : MessageSender
{
    public MessageService()
    {
        Debug = true;
    }
}