using Moonlight.App.Database.Entities;
using Moonlight.App.Event;

namespace Moonlight.App.Services.Background;

public class AutoMailSendService // This service is responsible for sending mails automatically 
{
    private readonly MailService MailService;
    private readonly ConfigService ConfigService;

    public AutoMailSendService(MailService mailService, ConfigService configService)
    {
        MailService = mailService;
        ConfigService = configService;
        
        Events.OnUserRegistered += OnUserRegistered;
    }

    private async void OnUserRegistered(object? _, User user)
    {
        await MailService.Send(user, $"Welcome {user.Username}", "welcome", user);
    }
}