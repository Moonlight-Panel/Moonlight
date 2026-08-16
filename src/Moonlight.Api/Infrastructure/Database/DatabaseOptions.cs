namespace Moonlight.Api.Infrastructure.Database;

public class DatabaseOptions
{
    public string Host { get; set; }
    public int Port { get; set; } = 5432;
    public string Username { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
}