using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Moonlight.App.Database.Entities;
using Moonlight.App.Repositories;
using Moonlight.App.Services.Interop;

namespace Moonlight.App.Services.Sessions;

public class SessionClientService
{
    public readonly Guid Uuid = Guid.NewGuid();
    public readonly DateTime CreateTimestamp = DateTime.UtcNow;
    public User? User { get; private set; }
    public string Ip { get; private set; } = "N/A";
    public string Device { get; private set; } = "N/A";

    public readonly IdentityService IdentityService;
    public readonly AlertService AlertService;
    public readonly NavigationManager NavigationManager;
    public readonly IJSRuntime JsRuntime;

    private readonly SessionServerService SessionServerService;
    private readonly Repository<User> UserRepository;

    public SessionClientService(
        IdentityService identityService,
        AlertService alertService,
        NavigationManager navigationManager,
        IJSRuntime jsRuntime,
        SessionServerService sessionServerService,
        Repository<User> userRepository)
    {
        IdentityService = identityService;
        AlertService = alertService;
        NavigationManager = navigationManager;
        JsRuntime = jsRuntime;
        SessionServerService = sessionServerService;
        UserRepository = userRepository;
    }

    public async Task Start()
    {
        User = await IdentityService.Get();
        Ip = IdentityService.GetIp();
        Device = IdentityService.GetDevice();

        if (User != null) // Track users last visit
        {
            User.LastVisitedAt = DateTime.UtcNow;
            UserRepository.Update(User);
        }

        await SessionServerService.Register(this);
    }

    public async Task Stop()
    {
        await SessionServerService.UnRegister(this);
    }
}