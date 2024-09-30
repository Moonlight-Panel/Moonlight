using Moonlight.ApiServer.Helpers;

namespace Moonlight.ApiServer.Database;

public class CoreDataContext : DatabaseContext
{
    public override string Prefix { get; } = "Core";
}