using Microsoft.AspNetCore.Components;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Models.Abstractions.Services;

public abstract class ServiceDefinition
{
    // Config
    public abstract ServiceActions Actions { get; }
    public abstract Type ConfigType { get; }
    
    // Methods
    public abstract Task BuildUserView(ServiceViewContext context);
    public abstract Task BuildAdminView(ServiceViewContext context);
}