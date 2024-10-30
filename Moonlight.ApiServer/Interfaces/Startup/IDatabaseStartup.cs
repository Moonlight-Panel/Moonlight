using Moonlight.ApiServer.Helpers;

namespace Moonlight.ApiServer.Interfaces.Startup;

public interface IDatabaseStartup
{
    public Task ConfigureDatabase(DatabaseContextCollection collection);
}