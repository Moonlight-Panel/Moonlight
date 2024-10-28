using Moonlight.ApiServer.Database;
using Moonlight.ApiServer.Helpers;
using Moonlight.ApiServer.Interfaces.Startup;

namespace Moonlight.ApiServer.Implementations.Startup;

public class CoreDatabaseStartup : IDatabaseStartup
{
    public Task ConfigureDatabase(DatabaseContextCollection collection)
    {
        collection.Add<CoreDataContext>();
        
        return Task.CompletedTask;
    }
}