using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moonlight.ApiSdk.Features.Roles;
using Moonlight.ApiSdk.Features.Users;

namespace Moonlight.Api.Infrastructure.Database;

public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RoleMembership> RoleMemberships { get; set; }
    
    private readonly IOptions<DatabaseOptions> _options;
    
    public DataContext(IOptions<DatabaseOptions> options)
    {
        _options = options;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(optionsBuilder.IsConfigured) return;

        optionsBuilder.UseNpgsql(
            $"Host={_options.Value.Host};" +
            $"Port={_options.Value.Port};" +
            $"Username={_options.Value.Username};" +
            $"Password={_options.Value.Password};" +
            $"Database={_options.Value.Name}"
        );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Model.SetDefaultSchema("core");
        base.OnModelCreating(modelBuilder);
    }
}