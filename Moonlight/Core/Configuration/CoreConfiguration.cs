using System.ComponentModel;
using MoonCore.Helpers;
using Newtonsoft.Json;

namespace Moonlight.Core.Configuration;

public class CoreConfiguration
{
    [JsonProperty("AppUrl")]
    [Description("This defines the public url of moonlight. This will be used e.g. by the nodes to communicate with moonlight")]
    public string AppUrl { get; set; } = "";

    [JsonProperty("Http")] public HttpData Http { get; set; } = new();
    [JsonProperty("Database")] public DatabaseData Database { get; set; } = new();
    [JsonProperty("Features")] public FeaturesData Features { get; set; } = new();
    [JsonProperty("Authentication")] public AuthenticationData Authentication { get; set; } = new();
    [JsonProperty("Customisation")] public CustomisationData Customisation { get; set; } = new();

    [JsonProperty("Security")] public SecurityData Security { get; set; } = new();
    [JsonProperty("Development")] public DevelopmentData Development { get; set; } = new();
    
    public class DevelopmentData
    {
        [JsonProperty("EnableApiReference")]
        [Description("This enables the api reference at your-moonlight.domain/admin/api/reference. Changing this requires a restart")]
        public bool EnableApiReference { get; set; } = false;
    }

    public class HttpData
    {
        [Description("The port moonlight should listen to http requests")]
        [JsonProperty("HttpPort")]
        public int HttpPort { get; set; } = 80;
        
        [Description("The port moonlight should listen to https requests if ssl is enabled")]
        [JsonProperty("HttpsPort")]
        public int HttpsPort { get; set; } = 443;
        
        [Description("Enables the use of an ssl certificate which is required in order to acceppt https requests")]
        [JsonProperty("EnableSsl")]
        public bool EnableSsl { get; set; } = false;

        [Description("Specifies the location of the certificate .pem file to load")]
        [JsonProperty("CertPath")]
        public string CertPath { get; set; } = "";
        
        [Description("Specifies the location of the key .pem file to load")]
        [JsonProperty("KeyPath")]
        public string KeyPath { get; set; } = "";

        [Description("Specifies the file upload limit per http request in megabytes")]
        [JsonProperty("UploadLimit")]
        public int UploadLimit { get; set; } = 100;

        [Description(
            "Specifies the maximum message size moonlight can receive via the websocket connection in kilobytes")]
        [JsonProperty("MessageSizeLimit")]
        public int MessageSizeLimit { get; set; } = 1024;
    }
    
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

        [JsonProperty("DenyRegister")]
        [Description("This disables the register function. No user will be able to sign up anymore. Its recommended to enable this for private instances")]
        public bool DenyRegister { get; set; } = false;
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

        [JsonProperty("Footer")] public FooterData Footer { get; set; } = new();
        [JsonProperty("CookieConsentBanner")] public CookieData CookieConsentBanner{ get; set; } = new();
    }
    
    public class FooterData
    {
        [Description("The name of the copyright holder. If this is changed from the default value, an additional 'Software by' will be shown")]
        public string CopyrightText { get; set; } = "Moonlight Panel";
        
        [Description("The link of the copyright holders website. If this is changed from the default value, an additional 'Software by' will be shown")]
        public string CopyrightLink { get; set; } = "https://moonlightpanel.xyz";

        [Description("A link to your 'about us' page. Leave it empty if you want to hide it")]
        public string AboutLink { get; set; } = "https://moonlightpanel.xyz";
        
        [Description("A link to your 'privacy' page. Leave it empty if you want to hide it")]
        public string PrivacyLink { get; set; } = "https://moonlightpanel.xyz";
        
        [Description("A link to your 'imprint' page. Leave it empty if you want to hide it")]
        public string ImprintLink { get; set; } = "https://moonlightpanel.xyz";
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
    
    public class CookieData
    {
        [JsonProperty("Enabled")]
        [Description("This specifies if the cookie consent banner is shown to users.")]
        public bool Enabled { get; set; } = false;

        [JsonProperty("BannerTitle")]
        [Description("The title for the cookie consent banner.")]
        public string BannerTitle { get; set; } = "\ud83c\udf6a Cookies";
        
        [JsonProperty("BannerText")]
        [Description("The description for the cookie consent banner.")]
        public string BannerText { get; set; } = "Moonlight is using cookies \ud83c\udf6a, to personalize your experience.";
        
        [JsonProperty("ConsentText")]
        [Description("The text for the consent option.")]
        public string ConsentText { get; set; } = "Consent";
        
        [JsonProperty("DeclineText")]
        [Description("The text for the decline option.")]
        public string DeclineText { get; set; } = "Decline";
    }
}