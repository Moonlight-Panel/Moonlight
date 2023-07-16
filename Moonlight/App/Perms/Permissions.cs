namespace Moonlight.App.Perms;

public static class Permissions
{
    public static Permission AdminDashboard = new()
    {
        Index = 0,
        Name = "Admin dashboard",
        Description = "See basic information about growth and status of the moonlight instance"
    };
    
    public static Permission SystemDashboard = new()
    {
        Index = 1,
        Name = "System information",
        Description = "See information about the moonlight instance like the uptime and memory usage"
    };
    
    public static Permission SystemSentry = new()
    {
        Index = 2,
        Name = "Settings for Sentry",
        Description = "See information about the sentry status"
    };
    
    public static Permission SystemMalware = new()
    {
        Index = 3,
        Name = "Server malware scanner",
        Description = "Scan running servers for malware"
    };
    
    public static Permission SystemSecurity = new()
    {
        Index = 4,
        Name = "System security settings",
        Description = "Ban ip addresses and view the security logs"
    };
    
    public static Permission SystemResources = new()
    {
        Index = 5,
        Name = "Resources",
        Description = "Read and write moonlight resources like configuration files"
    };
    
    public static Permission DiscordBot = new()
    {
        Index = 6,
        Name = "Discord bot actions",
        Description = "Setup and remote control the discord bot if enabled"
    };
    
    public static Permission NewsMessages = new()
    {
        Index = 7,
        Name = "News messages",
        Description = "Edit, view and delete messages for the user dashboard"
    };
    
    public static Permission SystemConfiguration = new()
    {
        Index = 8,
        Name = "System configuration",
        Description = "Modify the moonlight configuration though the visual editor"
    };
    
    public static Permission SystemMail = new()
    {
        Index = 9,
        Name = "System mail settings",
        Description = "Modify the mail templates and send test mails"
    };
    
    public static Permission ServersOverview = new()
    {
        Index = 10,
        Name = "Servers overview",
        Description = "View all servers and their owners"
    };
    
    public static Permission ServerAdminEdit = new()
    {
        Index = 11,
        Name = "Edit servers",
        Description = "View all servers and their owners"
    };
    
    public static Permission ServerManager = new()
    {
        Index = 12,
        Name = "Server manager",
        Description = "View all servers are currently running and stop/kill all running servers"
    };
    
    public static Permission ServerCleanup = new()
    {
        Index = 13,
        Name = "Server cleanup",
        Description = "View the stats about the cleanup system"
    };
    
    public static Permission Nodes = new()
    {
        Index = 14,
        Name = "Nodes",
        Description = "View stats about the nodes"
    };

    public static Permission? FromString(string name)
    {
        var type = typeof(Permissions);

        var field = type
            .GetFields()
            .FirstOrDefault(x => x.FieldType == typeof(Permission) && x.Name == name);

        if (field != null)
        {
            var value = field.GetValue(null);
            return value as Permission;
        }

        return null;
    }

    public static Permission[] GetAllPermissions()
    {
        var type = typeof(Permissions);

        return type
            .GetFields()
            .Where(x => x.FieldType == typeof(Permission))
            .Select(x => (x.GetValue(null) as Permission)!)
            .ToArray();
    }
}