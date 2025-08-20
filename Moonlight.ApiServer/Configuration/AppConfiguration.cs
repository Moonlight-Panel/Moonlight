using MoonCore.Helpers;
using YamlDotNet.Serialization;

namespace Moonlight.ApiServer.Configuration;

public record AppConfiguration
{
    [YamlMember(Description = "The public url your instance should be accessible through")]
    public string PublicUrl { get; set; } = "http://localhost:5165";
    
    [YamlMember(Description = "The credentials of the postgres which moonlight should use")]
    public DatabaseConfig Database { get; set; } = new();
    
    [YamlMember(Description = "Settings regarding authentication")]
    public AuthenticationConfig Authentication { get; set; } = new();
    
    [YamlMember(Description = "These options are only meant for development purposes")]
    public DevelopmentConfig Development { get; set; } = new();
    public FrontendData Frontend { get; set; } = new();
    public KestrelConfig Kestrel { get; set; } = new();
    public MetricsData Metrics { get; set; } = new();

    public static AppConfiguration CreateEmpty()
    {
        return new AppConfiguration()
        {
            // Set arrays as empty here
            
            Kestrel = new()
            {
                AllowedOrigins = []
            }
        };
    }

    public record FrontendData
    {
        [YamlMember(Description = "Enable the hosting of the frontend. Disable this if you only want to run the api server")]
        public bool EnableHosting { get; set; } = true;
    }
    
    public record DatabaseConfig
    {
        public string Host { get; set; } = "your-database-host.name";
        public int Port { get; set; } = 5432;

        public string Username { get; set; } = "db_user";
        public string Password { get; set; } = "db_password";

        public string Database { get; set; } = "db_name";
    }
    
    public record AuthenticationConfig
    {
        [YamlMember(Description = "The secret token to use for creating jwts and encrypting things. This needs to be at least 32 characters long")]
        public string Secret { get; set; } = Formatter.GenerateString(32);

        [YamlMember(Description = "Settings for the user sessions")]
        public SessionsConfig Sessions { get; set; } = new();
        
        [YamlMember(Description = "This specifies if the first registered/synced user will become an admin automatically")]
        public bool FirstUserAdmin { get; set; } = true;
    }

    public record SessionsConfig
    {
        public string CookieName { get; set; } = "session";
        public int ExpiresIn { get; set; } = 10;
    }
    
    public record DevelopmentConfig
    {
        [YamlMember(Description = "This toggles the availability of the api docs via /api/swagger")]
        public bool EnableApiDocs { get; set; } = false;
    }
    
    public record KestrelConfig
    {
        [YamlMember(Description = "The upload limit in megabytes for the api server")]
        public int UploadLimit { get; set; } = 100;
        
        [YamlMember(Description = "The allowed origins for the api server. Use * to allow all origins (which is not advised)")]
        public string[] AllowedOrigins { get; set; } = ["*"];
    }
    
    public record MetricsData
    {
        [YamlMember(Description = "This enables the collecting of metrics and allows access to the /metrics endpoint")]
        public bool Enable { get; set; } = false;
        
        [YamlMember(Description = "The interval in which metrics are created, specified in seconds")]
        public int Interval { get; set; } = 15;
    }
}