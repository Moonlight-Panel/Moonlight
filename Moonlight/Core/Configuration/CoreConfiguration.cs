using System.ComponentModel;
using MoonCore.Helpers;
using Newtonsoft.Json;

namespace Moonlight.Core.Configuration;

public class CoreConfiguration
{
    [JsonProperty("Database")] public DatabaseData Database { get; set; } = new();
    [JsonProperty("Features")] public FeaturesData Features { get; set; } = new();
    [JsonProperty("Authentication")] public AuthenticationData Authentication { get; set; } = new();
    [JsonProperty("Customisation")] public CustomisationData Customisation { get; set; } = new();

    [JsonProperty("Security")] public SecurityData Security { get; set; } = new();

    public class DatabaseData
    {
        [JsonProperty("Host")] public string Host { get; set; } = "your.db.host";

        [JsonProperty("Port")] public int Port { get; set; } = 3306;

        [JsonProperty("Username")] public string Username { get; set; } = "moonlight_user";

        [JsonProperty("Password")] public string Password { get; set; } = "s3cr3t";

        [JsonProperty("Database")] public string Database { get; set; } = "moonlight_db";
    }

    public class FeaturesData
    {
        [JsonProperty("DisableFeatures")] public List<string> DisableFeatures { get; set; } = new();
    }

    public class AuthenticationData
    {
        [JsonProperty("UseDefaultAuthentication")]
        [Description(
            "This enables the default authentication to be used. If you want to use a custom authentication plugin for e.g. LDAP disable this value")]
        public bool UseDefaultAuthentication { get; set; } = true;

        [JsonProperty("TokenDuration")]
        [Description("This specifies the duration the token of an user will be valid. The value specifies the days")]
        public int TokenDuration { get; set; } = 30;
    }

    public class SecurityData
    {
        [JsonProperty("Token")]
        [Description(
            "The token moonlight uses to encrypt everything and validate sessions. Do NOT share this with anyone")]
        public string Token { get; set; } = Formatter.GenerateString(32);

        [JsonProperty("DisableClientSideUnload")]
        [Description(
            "This disables the client side unload event from being handled as user might be able to hide their sessions with it. This will make the sessions inacurate and very slowly updated as blazor is not disposing instantly")]
        public bool DisableClientSideUnload { get; set; } = false;

        [JsonProperty("EnforceAvatarPrivacy")]
        [Description(
            "If set to true, this option will prevent users to see the avatars of other users using their id at the avatar endpoint. Users with the manage user permission bypass this check")]
        public bool EnforceAvatarPrivacy { get; set; } = true;
    }

    public class CustomisationData
    {
        [JsonProperty("DisableTimeBasedGreetingMessages")]
        [Description("This toggle disables the time based greeting messages")]
        public bool DisableTimeBasedGreetingMessages { get; set; } = false;

        [JsonProperty("GreetingTimezoneDifference")]
        [Description("This number specifies the hours from utc (can be negative) to use for the greeting messages")]
        public int GreetingTimezoneDifference { get; set; } = 2;

        [JsonProperty("FileManager")]
        public FileManagerData FileManager { get; set; } = new();
    }
    
    public class FileManagerData
    {
        [JsonProperty("MaxFileOpenSize")]
        [Description(
            "This specifies the maximum file size a user will be able to open in the file editor in kilobytes")]
        public int MaxFileOpenSize { get; set; } = 1024 * 5; // 5 MB

        [JsonProperty("OperationTimeout")]
        [Description("This specifies the general timeout for file manager operations. This can but has not to be used by file accesses")]
        public int OperationTimeout { get; set; } = 5;
    }
}