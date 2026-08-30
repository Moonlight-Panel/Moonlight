namespace Moonlight.FrontendSdk.Features.Auth;

public interface IPermissionProvider
{
    public Task ApplyChangesAsync(List<Permission> items);
}