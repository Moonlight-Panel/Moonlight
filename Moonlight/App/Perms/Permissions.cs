namespace Moonlight.App.Perms;

public static class Permissions
{
    public static Permission AdminDashboard = new()
    {
        Index = 0,
        Name = "Admin Dashboard",
        Description = "Access the main admin dashboard page"
    };

    public static Permission AdminStatistics = new()
    {
        Index = 1,
        Name = "Admin Statistics",
        Description = "View statistical information about the moonlight instance"
    };

    public static Permission AdminDomains = new()
    {
        Index = 4,
        Name = "Admin Domains",
        Description = "Manage domains in the admin area"
    };

    public static Permission AdminNewDomain = new()
    {
        Index = 5,
        Name = "Admin New Domain",
        Description = "Create a new domain in the admin area"
    };

    public static Permission AdminSharedDomains = new()
    {
        Index = 6,
        Name = "Admin Shared Domains",
        Description = "Manage shared domains in the admin area"
    };

    public static Permission AdminNewSharedDomain = new()
    {
        Index = 7,
        Name = "Admin New Shared Domain",
        Description = "Create a new shared domain in the admin area"
    };

    public static Permission AdminNodeDdos = new()
    {
        Index = 8,
        Name = "Admin Node DDoS",
        Description = "Manage DDoS protection for nodes in the admin area"
    };

    public static Permission AdminNodeEdit = new()
    {
        Index = 9,
        Name = "Admin Node Edit",
        Description = "Edit node settings in the admin area"
    };

    public static Permission AdminNodes = new()
    {
        Index = 10,
        Name = "Admin Node",
        Description = "Access the node management page in the admin area"
    };

    public static Permission AdminNewNode = new()
    {
        Index = 11,
        Name = "Admin New Node",
        Description = "Create a new node in the admin area"
    };

    public static Permission AdminNodeSetup = new()
    {
        Index = 12,
        Name = "Admin Node Setup",
        Description = "Set up a node in the admin area"
    };

    public static Permission AdminNodeView = new()
    {
        Index = 13,
        Name = "Admin Node View",
        Description = "View node details in the admin area"
    };

    public static Permission AdminNotificationDebugging = new()
    {
        Index = 14,
        Name = "Admin Notification Debugging",
        Description = "Manage debugging notifications in the admin area"
    };

    public static Permission AdminServerCleanup = new()
    {
        Index = 15,
        Name = "Admin Server Cleanup",
        Description = "Perform server cleanup tasks in the admin area"
    };

    public static Permission AdminServerEdit = new()
    {
        Index = 16,
        Name = "Admin Server Edit",
        Description = "Edit server settings in the admin area"
    };

    public static Permission AdminServers = new()
    {
        Index = 17,
        Name = "Admin Server",
        Description = "Access the server management page in the admin area"
    };

    public static Permission AdminServerManager = new()
    {
        Index = 18,
        Name = "Admin Server Manager",
        Description = "Manage servers in the admin area"
    };

    public static Permission AdminNewServer = new()
    {
        Index = 19,
        Name = "Admin New Server",
        Description = "Create a new server in the admin area"
    };

    public static Permission AdminServerImageEdit = new()
    {
        Index = 20,
        Name = "Admin Server Image Edit",
        Description = "Edit server image settings in the admin area"
    };

    public static Permission AdminServerImages = new()
    {
        Index = 21,
        Name = "Admin Server Images",
        Description = "Access the server image management page in the admin area"
    };

    public static Permission AdminServerImageNew = new()
    {
        Index = 22,
        Name = "Admin Server Image New",
        Description = "Create a new server image in the admin area"
    };

    public static Permission AdminServerViewAllocations = new()
    {
        Index = 23,
        Name = "Admin Server View Allocations",
        Description = "View server allocations in the admin area"
    };

    public static Permission AdminServerViewArchive = new()
    {
        Index = 24,
        Name = "Admin Server View Archive",
        Description = "View server archive in the admin area"
    };

    public static Permission AdminServerViewDebug = new()
    {
        Index = 25,
        Name = "Admin Server View Debug",
        Description = "View server debugging information in the admin area"
    };

    public static Permission AdminServerViewImage = new()
    {
        Index = 26,
        Name = "Admin Server View Image",
        Description = "View server image details in the admin area"
    };

    public static Permission AdminServerViewIndex = new()
    {
        Index = 27,
        Name = "Admin Server View",
        Description = "Access the server view page in the admin area"
    };

    public static Permission AdminServerViewOverview = new()
    {
        Index = 28,
        Name = "Admin Server View Overview",
        Description = "View server overview in the admin area"
    };

    public static Permission AdminServerViewResources = new()
    {
        Index = 29,
        Name = "Admin Server View Resources",
        Description = "View server resources in the admin area"
    };

    public static Permission AdminSubscriptionEdit = new()
    {
        Index = 30,
        Name = "Admin Subscription Edit",
        Description = "Edit subscription settings in the admin area"
    };

    public static Permission AdminSubscriptions = new()
    {
        Index = 31,
        Name = "Admin Subscriptions",
        Description = "Access the subscription management page in the admin area"
    };

    public static Permission AdminNewSubscription = new()
    {
        Index = 32,
        Name = "Admin New Subscription",
        Description = "Create a new subscription in the admin area"
    };

    public static Permission AdminSupport = new()
    {
        Index = 33,
        Name = "Admin Support",
        Description = "Access the support page in the admin area"
    };

    public static Permission AdminSupportView = new()
    {
        Index = 34,
        Name = "Admin Support View",
        Description = "View support details in the admin area"
    };

    public static Permission AdminSysConfiguration = new()
    {
        Index = 35,
        Name = "Admin system Configuration",
        Description = "Access system configuration settings in the admin area"
    };

    public static Permission AdminSysDiscordBot = new()
    {
        Index = 36,
        Name = "Admin system Discord Bot",
        Description = "Manage Discord bot settings in the admin area"
    };

    public static Permission AdminSystem = new()
    {
        Index = 37,
        Name = "Admin system",
        Description = "Access the system management page in the admin area"
    };

    public static Permission AdminSysMail = new()
    {
        Index = 38,
        Name = "Admin system Mail",
        Description = "Manage mail settings in the admin area"
    };

    public static Permission AdminSysMalware = new()
    {
        Index = 39,
        Name = "Admin system Malware",
        Description = "Manage malware settings in the admin area"
    };

    public static Permission AdminSysResources = new()
    {
        Index = 40,
        Name = "Admin system Resources",
        Description = "View system resources in the admin area"
    };

    public static Permission AdminSysSecurity = new()
    {
        Index = 41,
        Name = "Admin system Security",
        Description = "Manage security settings in the admin area"
    };

    public static Permission AdminSysSentry = new()
    {
        Index = 42,
        Name = "Admin system Sentry",
        Description = "Manage Sentry settings in the admin area"
    };

    public static Permission AdminSysNewsEdit = new()
    {
        Index = 43,
        Name = "Admin system News Edit",
        Description = "Edit system news in the admin area"
    };

    public static Permission AdminSysNews = new()
    {
        Index = 44,
        Name = "Admin system News",
        Description = "Access the system news management page in the admin area"
    };

    public static Permission AdminSysNewsNew = new()
    {
        Index = 45,
        Name = "Admin system News New",
        Description = "Create new system news in the admin area"
    };

    public static Permission AdminUserEdit = new()
    {
        Index = 46,
        Name = "Admin User Edit",
        Description = "Edit user settings in the admin area"
    };

    public static Permission AdminUsers = new()
    {
        Index = 47,
        Name = "Admin Users",
        Description = "Access the user management page in the admin area"
    };

    public static Permission AdminNewUser = new()
    {
        Index = 48,
        Name = "Admin New User",
        Description = "Create a new user in the admin area"
    };

    public static Permission AdminUserSessions = new()
    {
        Index = 49,
        Name = "Admin User Sessions",
        Description = "View user sessions in the admin area"
    };

    public static Permission AdminUserView = new()
    {
        Index = 50,
        Name = "Admin User View",
        Description = "View user details in the admin area"
    };

    public static Permission AdminWebspaces = new()
    {
        Index = 51,
        Name = "Admin Webspaces",
        Description = "Access the webspaces management page in the admin area"
    };

    public static Permission AdminNewWebspace = new()
    {
        Index = 52,
        Name = "Admin New Webspace",
        Description = "Create a new webspace in the admin area"
    };

    public static Permission AdminWebspacesServerEdit = new()
    {
        Index = 53,
        Name = "Admin Webspaces Server Edit",
        Description = "Edit webspace server settings in the admin area"
    };

    public static Permission AdminWebspacesServers = new()
    {
        Index = 54,
        Name = "Admin Webspaces Servers",
        Description = "Access the webspace server management page in the admin area"
    };

    public static Permission AdminWebspacesServerNew = new()
    {
        Index = 55,
        Name = "Admin Webspaces Server New",
        Description = "Create a new webspace server in the admin area"
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