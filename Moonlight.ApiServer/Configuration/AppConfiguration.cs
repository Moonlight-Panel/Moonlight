namespace Moonlight.ApiServer.Configuration;

public class AppConfiguration
{
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
        public string Secret { get; set; } = Guid
            .NewGuid()
            .ToString()
            .Replace("-", "");

        public int TokenDuration { get; set; } = 10;
    }
    
    public class DevelopmentConfig
    {
        public bool EnableApiDocs { get; set; } = false;
    }
}