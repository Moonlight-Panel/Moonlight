using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Services;

namespace Moonlight.App.Database;

public class DataContext : DbContext
{
    private readonly ConfigService ConfigService;
    
    public DataContext(ConfigService configService)
    {
        ConfigService = configService;
    }

    public DbSet<DockerImage> DockerImages { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<ImageTag> ImageTags { get; set; }
    public DbSet<ImageVariable> ImageVariables { get; set; }
    public DbSet<Node> Nodes { get; set; }
    public DbSet<NodeAllocation> NodeAllocations { get; set; }
    public DbSet<Server> Servers { get; set; }
    public DbSet<ServerBackup> ServerBackups { get; set; }
    public DbSet<ServerVariable> ServerVariables { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<LoadingMessage> LoadingMessages { get; set; }
    public DbSet<AuditLogEntry> AuditLog { get; set; }
    public DbSet<Entities.Database> Databases { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = ConfigService
                .GetSection("Moonlight")
                .GetSection("Database");

            var connectionString = $"host={config.GetValue<string>("Host")};" +
                                   $"port={config.GetValue<int>("Port")};" +
                                   $"database={config.GetValue<string>("Database")};" +
                                   $"uid={config.GetValue<string>("Username")};" +
                                   $"pwd={config.GetValue<string>("Password")}";
            
            optionsBuilder.UseMySql(
                connectionString,
                ServerVersion.Parse("5.7.37-mysql"),
                builder =>
                {
                    builder.EnableRetryOnFailure(5);
                }
            );
        }
    }
}