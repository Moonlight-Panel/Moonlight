namespace Moonlight.ApiServer.App.Configuration;

public class AppConfiguration
{
    public DatabaseConfig Database { get; set; } = new();
    public SecurityConfig Security { get; set; } = new();
    public AuthenticationConfig Authentication { get; set; } = new();
    
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
        public int TokenDuration { get; set; } = 10;
    }
    
    public class SecurityConfig
    {
        public string Token { get; set; } = Guid
            .NewGuid()
            .ToString()
            .Replace("-", "");
    }
}