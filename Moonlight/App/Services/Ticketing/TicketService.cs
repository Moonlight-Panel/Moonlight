namespace Moonlight.App.Services.Ticketing;

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