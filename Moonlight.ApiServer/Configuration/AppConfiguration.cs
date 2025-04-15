using MoonCore.Helpers;

namespace Moonlight.ApiServer.Configuration;

public class AppConfiguration
{
    public string PublicUrl { get; set; } = "http://localhost:5165";
    
    public DatabaseConfig Database { get; set; } = new();
    public AuthenticationConfig Authentication { get; set; } = new();
    public DevelopmentConfig Development { get; set; } = new();
    public ClientConfig Client { get; set; } = new();
    public KestrelConfig Kestrel { get; set; } = new();

    public class ClientConfig
    {
        public bool Enable { get; set; } = true;
    }
    
    public class DatabaseConfig
    {
        public string Host { get; set; } = "your-database-host.name";
        public int Port { get; set; } = 5432;

        public string Username { get; set; } = "db_user";
        public string Password { get; set; } = "db_password";

        public string Database { get; set; } = "db_name";
    }
    
    public class AuthenticationConfig
    {
        public string Secret { get; set; } = Formatter.GenerateString(32);
        public int TokenDuration { get; set; } = 24 * 10;

        public bool EnableLocalOAuth2 { get; set; } = true;
        public OAuth2Data OAuth2 { get; set; } = new();
        
        public class OAuth2Data
        {
            public string Secret { get; set; } = Formatter.GenerateString(32);
            public string ClientId { get; set; } = Formatter.GenerateString(8);
            public string ClientSecret { get; set; } = Formatter.GenerateString(32);
            public string? AuthorizationEndpoint { get; set; }
            public string? AccessEndpoint { get; set; }
            public string? AuthorizationRedirect { get; set; }
        }
    }
    
    public class DevelopmentConfig
    {
        public bool EnableApiDocs { get; set; } = false;
    }
    
    public class KestrelConfig
    {
        public int UploadLimit { get; set; } = 100;
    }
}