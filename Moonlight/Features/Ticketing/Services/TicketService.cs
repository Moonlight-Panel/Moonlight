using MoonCore.Attributes;

namespace Moonlight.Features.Ticketing.Services;

[Scoped]
public class TicketService
{
    private readonly IServiceProvider ServiceProvider;

    public TicketChatService Chat => ServiceProvider.GetRequiredService<TicketChatService>();
    public TicketCreateService Create => ServiceProvider.GetRequiredService<TicketCreateService>();

    public TicketService(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
}