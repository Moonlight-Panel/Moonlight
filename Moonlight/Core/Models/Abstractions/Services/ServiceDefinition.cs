namespace Moonlight.Core.Models.Abstractions.Services;

public abstract class ServiceDefinition
{
    // Config
    public abstract ServiceActions Actions { get; }
    public abstract Type ConfigType { get; }
    
    // Methods
    public abstract Task BuildUserView(ServiceViewContext context);
    public abstract Task BuildAdminView(ServiceViewContext context);
}