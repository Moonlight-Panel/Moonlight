using Moonlight.Core.Database.Entities;
using Moonlight.Core.Event;
using Moonlight.Core.Event.Args;
using Moonlight.Features.ServiceManagement.Entities;
using BackgroundService = MoonCore.Abstractions.BackgroundService;

namespace Moonlight.Core.Services.Background;

public class AutoMailSendService : BackgroundService // This service is responsible for sending mails automatically 
{
    private readonly MailService MailService;

    public AutoMailSendService(MailService mailService)
    {
        MailService = mailService;
    }
    
    public override Task Run()
    {
        Events.OnUserRegistered += OnUserRegistered;
        Events.OnServiceOrdered += OnServiceOrdered;
        Events.OnTransactionCreated += OnTransactionCreated;
        
        return Task.CompletedTask;
    }

    private async void OnTransactionCreated(object? sender, TransactionCreatedEventArgs eventArgs)
    {
        await MailService.Send(
            eventArgs.User,
            "New transaction",
            "transactionCreated",
            eventArgs.Transaction,
            eventArgs.User
        );
    }

    private async void OnServiceOrdered(object? _, Service service)
    {
        await MailService.Send(
            service.Owner,
            "New product ordered",
            "serviceOrdered",
            service,
            service.Product,
            service.Owner
        );
    }

    private async void OnUserRegistered(object? _, User user)
    {
        await MailService.Send(
            user,
            $"Welcome {user.Username}",
            "welcome",
            user
        );
    }
}