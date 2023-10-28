using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Community;
using Moonlight.App.Database.Entities.Store;
using Moonlight.App.Database.Entities.Tickets;
using Moonlight.App.Services;

namespace Moonlight.App.Database;

public class DataContext : DbContext
{
    private readonly ConfigService ConfigService;
    
    public DbSet<User> Users { get; set; }
    //public DbSet<Ticket> Tickets { get; set; }
    //public DbSet<TicketMessage> TicketMessages { get; set; }
    
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

    public DataContext(ConfigService configService)
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