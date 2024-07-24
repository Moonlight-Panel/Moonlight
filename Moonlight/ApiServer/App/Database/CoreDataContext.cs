using MoonCore.Services;
using Moonlight.ApiServer.App.Configuration;
using Moonlight.ApiServer.App.Helpers.Database;

namespace Moonlight.ApiServer.App.Database;

public class CoreDataContext : DatabaseContext
{
    public override string Prefix => "Core";
    
    
    
    public CoreDataContext(ConfigService<AppConfiguration> configService) : base(configService)
    {
    }
}