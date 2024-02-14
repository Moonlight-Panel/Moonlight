using Microsoft.EntityFrameworkCore;
using MoonCore.Services;
using Moonlight.Core.Configuration;
using Moonlight.Core.Database.Entities;
using Moonlight.Core.Services;
using Moonlight.Features.Community.Entities;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.ServiceManagement.Entities;
using Moonlight.Features.StoreSystem.Entities;
using Moonlight.Features.Theming.Entities;
using Moonlight.Features.Ticketing.Entities;

namespace Moonlight.Core.Database;

public class DataContext : DbContext
{
    private readonly ConfigService<ConfigV1> ConfigService;
    
    public DbSet<User> Users { get; set; }
    
    // Store
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<ServiceShare> ServiceShares { get; set; }
    
    public DbSet<GiftCode> GiftCodes { get; set; }
    public DbSet<GiftCodeUse> GiftCodeUses { get; set; }
    
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<CouponUse> CouponUses { get; set; }
    
    // Community
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostComment> PostComments { get; set; }
    public DbSet<PostLike> PostLikes { get; set; }
    public DbSet<WordFilter> WordFilters { get; set; }
    
    // Tickets
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketMessage> TicketMessages { get; set; }
    
    // Themes
    public DbSet<Theme> Themes { get; set; }
    
    // Servers
    public DbSet<Server> Servers { get; set; }
    public DbSet<ServerAllocation> ServerAllocations { get; set; }
    public DbSet<ServerImage> ServerImages { get; set; }
    public DbSet<ServerNode> ServerNodes { get; set; }
    public DbSet<ServerVariable> ServerVariables { get; set; }
    public DbSet<ServerDockerImage> ServerDockerImages { get; set; }
    public DbSet<ServerImageVariable> ServerImageVariables { get; set; }
    public DbSet<ServerSchedule> ServerSchedules { get; set; }

    public DataContext(ConfigService<ConfigV1> configService)
    {
        ConfigService = configService;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = ConfigService.Get().Database;

            if (config.UseSqlite)
                optionsBuilder.UseSqlite($"Data Source={config.SqlitePath}");
            else
            {
                var connectionString = $"host={config.Host};" +
                                       $"port={config.Port};" +
                                       $"database={config.Database};" +
                                       $"uid={config.Username};" +
                                       $"pwd={config.Password}";

                optionsBuilder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    builder => builder.EnableRetryOnFailure(5)
                );
            }
        }
    }
}