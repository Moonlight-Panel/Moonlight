using Microsoft.EntityFrameworkCore;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Helpers;

namespace Moonlight.ApiServer.Database;

public class CoreDataContext : DatabaseContext
{
    public override string Prefix { get; } = "Core";

    public DbSet<User> Users { get; set; }
}