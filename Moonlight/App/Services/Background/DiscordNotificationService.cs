using Discord;
using Discord.Webhook;
using Moonlight.App.Database.Entities;
using Moonlight.App.Events;
using Moonlight.App.Helpers;
using Moonlight.App.Services.Files;

namespace Moonlight.App.Services.Background;

public class DiscordNotificationService
{
    private readonly EventSystem Event;
    private readonly ResourceService ResourceService;
    private readonly DiscordWebhookClient Client;
    private readonly string AppUrl;

    public DiscordNotificationService(
        EventSystem eventSystem,
        ConfigService configService,
        ResourceService resourceService)
    {
        Event = eventSystem;
        ResourceService = resourceService;

        var config = configService.Get().Moonlight.DiscordNotifications;

        if (config.Enable)
        {
            Logger.Info("Discord notifications enabled");
            
            Client = new(config.WebHook);
            AppUrl = configService.Get().Moonlight.AppUrl;

            Event.On<Ticket>("tickets.new", this, OnNewTicket);
            Event.On<Ticket>("tickets.status", this, OnTicketStatusUpdated);
            Event.On<User>("user.rating", this, OnUserRated);
            Event.On<User>("billing.completed", this, OnBillingCompleted);
            Event.On<BlocklistIp>("ddos.add", this, OnIpBlockListed);
        }
        else
        {
            Logger.Info("Discord notifications disabled");
        }
    }

    private async Task OnTicketStatusUpdated(Ticket ticket)
    {
        await SendNotification("", builder =>
        {
            builder.Title = "Ticket status has been updated";
            builder.AddField("Issue topic", ticket.IssueTopic);
            builder.AddField("Status", ticket.Status);

            if (ticket.AssignedTo != null)
            {
                builder.AddField("Assigned to", $"{ticket.AssignedTo.FirstName} {ticket.AssignedTo.LastName}");
            }
            
            builder.Color = Color.Green;
            builder.Url = $"{AppUrl}/admin/support/view/{ticket.Id}";
        });
    }

    private async Task OnIpBlockListed(BlocklistIp blocklistIp)
    {
        await SendNotification("", builder =>
        {
            builder.Color = Color.Red;
            builder.Title = "New ddos attack detected";

            builder.AddField("IP", blocklistIp.Ip);
            builder.AddField("Packets", blocklistIp.Packets);
        });
    }

    private async Task OnBillingCompleted(User user)
    {
        await SendNotification("", builder =>
        {
            builder.Color = Color.Red;
            builder.Title = "New payment received";

            builder.AddField("User", user.Email);
            builder.AddField("Firstname", user.FirstName);
            builder.AddField("Lastname", user.LastName);
            builder.AddField("Amount", user.CurrentSubscription!.Price);
            builder.AddField("Currency", user.CurrentSubscription!.Currency);
        });
    }

    private async Task OnUserRated(User user)
    {
        await SendNotification("", builder =>
        {
            builder.Color = Color.Gold;
            builder.Title = "New user rating";

            builder.AddField("User", user.Email);
            builder.AddField("Firstname", user.FirstName);
            builder.AddField("Lastname", user.LastName);
            builder.AddField("Rating", new string('⭐', user.Rating));
        });
    }

    private async Task OnNewTicket(Ticket ticket)
    {
        await SendNotification("", builder =>
        {
            builder.Title = "A new ticket has been created";
            builder.AddField("Issue topic", ticket.IssueTopic);
            builder.Color = Color.Green;
            builder.Url = $"{AppUrl}/admin/support/view/{ticket.Id}";
        });
    }

    private async Task SendNotification(string content, Action<EmbedBuilder>? embed = null)
    {
        var e = new EmbedBuilder();
        embed?.Invoke(e);

        await Client.SendMessageAsync(
            content, 
            false, 
            new []{e.Build()}, 
            "Moonlight Notification",
            ResourceService.Image("logo.svg")
        );
    }
}