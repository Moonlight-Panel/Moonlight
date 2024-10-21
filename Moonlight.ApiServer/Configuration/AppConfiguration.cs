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
        public string AccessSecret { get; set; } = Formatter.GenerateString(32);
        public string RefreshSecret { get; set; } = Formatter.GenerateString(32);
        public int AccessDuration { get; set; } = 60;
        public int RefreshDuration { get; set; } = 3600;

        public OAuth2Data OAuth2 { get; set; } = new();
        public bool UseLocalOAuth2 { get; set; } = true;
        
        public LocalOAuth2Data LocalOAuth2 { get; set; } = new();
        
        public class LocalOAuth2Data
        {
            public string AccessSecret { get; set; } = Formatter.GenerateString(32);
            public string RefreshSecret { get; set; } = Formatter.GenerateString(32);
            public string CodeSecret { get; set; } = Formatter.GenerateString(32);

            public int AccessTokenDuration { get; set; } = 60;
            public int RefreshTokenDuration { get; set; } = 3600;
        }
        
        public class OAuth2Data
        {
            public string ClientId { get; set; } = Formatter.GenerateString(8);
            public string ClientSecret { get; set; } = Formatter.GenerateString(32);
            public string? AuthorizationUri { get; set; }
            public string? AuthorizationRedirect { get; set; }
            public string? AccessEndpoint { get; set; }
            public string? RefreshEndpoint { get; set; }
        }
    }
    
    public class DevelopmentConfig
    {
        public bool EnableApiDocs { get; set; } = false;
    }
}