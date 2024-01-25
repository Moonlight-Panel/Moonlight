using Moonlight.Core.Database.Entities;
using Moonlight.Core.Database.Entities.Store;
using Moonlight.Core.Event;
using Moonlight.Core.Event.Args;

namespace Moonlight.Core.Services.Background;

public class AutoMailSendService // This service is responsible for sending mails automatically 
{
    private readonly MailService MailService;

    public AutoMailSendService(MailService mailService)
    {
        MailService = mailService;

        Events.OnUserRegistered += OnUserRegistered;
        Events.OnServiceOrdered += OnServiceOrdered;
        Events.OnTransactionCreated += OnTransactionCreated;
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