using Microsoft.AspNetCore.Components;
using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Store;

namespace Moonlight.App.Models.Abstractions;

public abstract class ServiceImplementation
{
    public abstract ServiceActions Actions { get; }
    public abstract Type ConfigType { get; }

    public abstract RenderFragment GetAdminLayout();
    public abstract RenderFragment GetUserLayout();
    
    // The service and user parameter can be used to only show certain pages to admins or other 
    public abstract ServiceUiPage[] GetUserPages(Service service, User user);
    public abstract ServiceUiPage[] GetAdminPages(Service service, User user);
}