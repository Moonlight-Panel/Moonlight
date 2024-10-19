using Moonlight.ApiServer.Database.Entities;

namespace Moonlight.ApiServer.Interfaces.Auth;

public interface IAuthInterceptor
{
    public bool AllowAccess(User user, IServiceProvider serviceProvider);
    public bool AllowRefresh(User user, IServiceProvider serviceProvider);
}