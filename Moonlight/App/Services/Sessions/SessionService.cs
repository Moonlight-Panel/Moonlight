using Microsoft.AspNetCore.Components;
using Moonlight.App.Database.Entities;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories;
using Moonlight.App.Services.Interop;

namespace Moonlight.App.Services.Sessions;

public class SessionService
{
    private readonly SessionRepository SessionRepository;
    private readonly IdentityService IdentityService;
    private readonly NavigationManager NavigationManager;
    private readonly AlertService AlertService;
    private readonly DateTimeService DateTimeService;

    private Session? OwnSession;

    public SessionService(
        SessionRepository sessionRepository, 
        IdentityService identityService, 
        NavigationManager navigationManager,
        AlertService alertService,
        DateTimeService dateTimeService)
    {
        SessionRepository = sessionRepository;
        IdentityService = identityService;
        NavigationManager = navigationManager;
        AlertService = alertService;
        DateTimeService = dateTimeService;
    }

    public async Task Register()
    {
        var user = await IdentityService.Get();
        
        OwnSession = new Session()
        {
            Ip = IdentityService.GetIp(),
            Url = NavigationManager.Uri,
            Device = IdentityService.GetDevice(),
            CreatedAt = DateTimeService.GetCurrent(),
            User = user,
            Navigation = NavigationManager,
            AlertService = AlertService
        };
        
        SessionRepository.Add(OwnSession);
    }

    public void Refresh()
    {
        OwnSession.Url = NavigationManager.Uri;
    }

    public void Close()
    {
        SessionRepository.Delete(OwnSession);
    }

    public Session[] GetAll()
    {
        return SessionRepository.Get();
    }

    public void ReloadUserSessions(User user)
    {
        foreach (var session in SessionRepository.Get())
        {
            if(session.User != null && session.User.Id == user.Id)
                session.Navigation.NavigateTo(session.Navigation.Uri, true);
        }
    }
}