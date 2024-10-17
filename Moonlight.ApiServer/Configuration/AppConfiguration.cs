using MoonCore.Helpers;

namespace Moonlight.ApiServer.Configuration;

public class AppConfiguration
{
    public string PublicUrl { get; set; } = "http://localhost:5165";
    
    public DatabaseConfig Database { get; set; } = new();
    public AuthenticationConfig Authentication { get; set; } = new();
    public DevelopmentConfig Development { get; set; } = new();
    
    public class DatabaseConfig
    {
        public string Host { get; set; } = "your-database-host.name";
        public int Port { get; set; } = 3306;

        public string Username { get; set; } = "db_user";
        public string Password { get; set; } = "db_password";

        public string Database { get; set; } = "db_name";
    }
    
    public class AuthenticationConfig
    {
        public string MlAccessSecret { get; set; } = Formatter.GenerateString(32);
        public string MlRefreshSecret { get; set; } = Formatter.GenerateString(32);
        
        public string Secret { get; set; } = Formatter.GenerateString(32);

        public int TokenDuration { get; set; } = 10;

        public bool UseLocalOAuth2Service { get; set; } = true;
        public string AccessSecret { get; set; } = Formatter.GenerateString(32);
        public string RefreshSecret { get; set; } = Formatter.GenerateString(32);
        public string ClientId { get; set; } = Formatter.GenerateString(8);
        public string ClientSecret { get; set; } = Formatter.GenerateString(32);
        public string? AuthorizationUri { get; set; }
        public string? AuthorizationRedirect { get; set; }
        public string? AccessEndpoint { get; set; }
        public string? RefreshEndpoint { get; set; }

        // Local OAuth2 Service
        public string CodeSecret { get; set; } = Formatter.GenerateString(32);
    }
    
    public class DevelopmentConfig
    {
        public bool EnableApiDocs { get; set; } = false;
    }
}