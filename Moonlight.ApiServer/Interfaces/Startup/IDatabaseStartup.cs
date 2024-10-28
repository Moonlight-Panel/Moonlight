using Moonlight.ApiServer.Helpers;
using Moonlight.ApiServer.Models;

namespace Moonlight.ApiServer.Interfaces.Startup;

public interface IDatabaseStartup
{
    public Task ConfigureDatabase(DatabaseContextCollection collection);
}