using System.ComponentModel;
using Moonlight.Core.Helpers;
using Moonlight.Features.Advertisement.Configuration;
using Moonlight.Features.StoreSystem.Configuration;
using Moonlight.Features.Theming.Configuration;
using Newtonsoft.Json;

namespace Moonlight.Core.Configuration;

public class ConfigV1
{
    [JsonProperty("AppUrl")]
    [Description("The url with which moonlight is accessible from the internet. It must not end with a /")]
    public string AppUrl { get; set; } = "http://your-moonlight-instance-without-slash.owo";
    
    [JsonProperty("Security")] public SecurityData Security { get; set; } = new();
    [JsonProperty("Database")] public DatabaseData Database { get; set; } = new();
    [JsonProperty("MailServer")] public MailServerData MailServer { get; set; } = new();

    [JsonProperty("Store")] public StoreData Store { get; set; } = new();

    [JsonProperty("Theme")] public ThemeData Theme { get; set; } = new();
    [JsonProperty("Advertisement")] public AdvertisementData Advertisement { get; set; } = new();
    
    public class SecurityData
    {
        [JsonProperty("Token")]
        [Description("The security token helio will use to encrypt various things like tokens")]
        public string Token { get; set; } = Guid.NewGuid().ToString().Replace("-", "");

        [JsonProperty("EnableEmailVerify")]
        [Description("This will users force to verify their email address if they havent already")]
        public bool EnableEmailVerify { get; set; } = false;

        [JsonProperty("EnableReverseProxyMode")]
        [Description("Enable this option if you are using a reverse proxy to access moonlight. This will configure some parts of moonlight to act correctly like the ip detection")]
        public bool EnableReverseProxyMode { get; set; } = false;
    }

    public class DatabaseData
    {
        [JsonProperty("UseSqlite")]
        public bool UseSqlite { get; set; } = false;
        
        [JsonProperty("SqlitePath")]
        public string SqlitePath { get; set; } = PathBuilder.File("storage", "data.sqlite");
        
        [JsonProperty("Host")]
        public string Host { get; set; } = "your.db.host";
        
        [JsonProperty("Port")]
        public int Port { get; set; } = 3306;
        
        [JsonProperty("Username")]
        public string Username { get; set; } = "moonlight_user";
        
        [JsonProperty("Password")]
        public string Password { get; set; } = "s3cr3t";
        
        [JsonProperty("Database")]
        public string Database { get; set; } = "moonlight_db";
    }
    
    public class MailServerData
    {
        [JsonProperty("Host")] public string Host { get; set; } = "your.email.host";

        [JsonProperty("Port")] public int Port { get; set; } = 465;

        [JsonProperty("Email")] public string Email { get; set; } = "noreply@your.email.host";

        [JsonProperty("Password")] public string Password { get; set; } = "s3cr3t";

        [JsonProperty("UseSsl")] public bool UseSsl { get; set; } = true;

        [JsonProperty("SenderName")]
        [Description("This will be shown as the system emails sender name in apps like gmail")]
        public string SenderName { get; set; } = "Moonlight System";
    }
}