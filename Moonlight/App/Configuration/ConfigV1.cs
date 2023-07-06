using System.ComponentModel;
using Moonlight.App.Helpers;

namespace Moonlight.App.Configuration;

using System;
using Newtonsoft.Json;

public class ConfigV1
{
    [JsonProperty("Moonlight")]
    public MoonlightData Moonlight { get; set; } = new();

    public class MoonlightData
    {
        [JsonProperty("AppUrl")]
        [Description("The url moonlight is accesible with from the internet")]
        public string AppUrl { get; set; } = "http://your-moonlight-url-without-slash";

        [JsonProperty("Database")] public DatabaseData Database { get; set; } = new();

        [JsonProperty("DiscordBotApi")] public DiscordBotApiData DiscordBotApi { get; set; } = new();

        [JsonProperty("DiscordBot")] public DiscordBotData DiscordBot { get; set; } = new();

        [JsonProperty("Domains")] public DomainsData Domains { get; set; } = new();

        [JsonProperty("Html")] public HtmlData Html { get; set; } = new();

        [JsonProperty("Marketing")] public MarketingData Marketing { get; set; } = new();

        [JsonProperty("OAuth2")] public OAuth2Data OAuth2 { get; set; } = new();

        [JsonProperty("Security")] public SecurityData Security { get; set; } = new();

        [JsonProperty("Mail")] public MailData Mail { get; set; } = new();

        [JsonProperty("Cleanup")] public CleanupData Cleanup { get; set; } = new();

        [JsonProperty("Subscriptions")] public SubscriptionsData Subscriptions { get; set; } = new();

        [JsonProperty("DiscordNotifications")]
        public DiscordNotificationsData DiscordNotifications { get; set; } = new();

        [JsonProperty("Statistics")] public StatisticsData Statistics { get; set; } = new();

        [JsonProperty("Rating")] public RatingData Rating { get; set; } = new();

        [JsonProperty("SmartDeploy")] public SmartDeployData SmartDeploy { get; set; } = new();

        [JsonProperty("Sentry")] public SentryData Sentry { get; set; } = new();
    }

    public class CleanupData
    {
        [JsonProperty("Cpu")]
        [Description("The maximum amount of cpu usage in percent a node is allowed to use before the cleanup starts")]
        public long Cpu { get; set; } = 90;

        [JsonProperty("Memory")]
        [Description("The minumum amount of memory in megabytes avaliable before the cleanup starts")]
        public long Memory { get; set; } = 8192;

        [JsonProperty("Wait")]
        [Description("The delay between every cleanup check in minutes")]
        public long Wait { get; set; } = 15;

        [JsonProperty("Uptime")]
        [Description("The maximum uptime of any server in hours before it the server restarted by the cleanup system")]
        public long Uptime { get; set; } = 6;

        [JsonProperty("Enable")]
        [Description("The cleanup system provides a fair way for stopping unused servers and staying stable even with overallocation. A detailed explanation: docs.endelon-hosting.de/erklaerungen/cleanup")]
        public bool Enable { get; set; } = false;

        [JsonProperty("MinUptime")]
        [Description("The minumum uptime of a server in minutes to prevent stopping servers which just started")]
        public long MinUptime { get; set; } = 10;
    }

    public class DatabaseData
    {
        [JsonProperty("Database")] public string Database { get; set; } = "moonlight_db";

        [JsonProperty("Host")] public string Host { get; set; } = "your.database.host";
        
        [JsonProperty("Password")]
        [Blur]
        public string Password { get; set; } = "secret";

        [JsonProperty("Port")] public long Port { get; set; } = 3306;

        [JsonProperty("Username")] public string Username { get; set; } = "moonlight_user";
    }

    public class DiscordBotApiData
    {
        [JsonProperty("Enable")]
        [Description("Enable the discord bot api. Currently only DatBot is using this api")]
        public bool Enable { get; set; } = false;

        [JsonProperty("Token")]
        [Description("Specify the token the api client needs to provide")]
        [Blur]
        public string Token { get; set; } = Guid.NewGuid().ToString();
    }
    public class DiscordBotData
    {
        [JsonProperty("Enable")]
        [Description("The discord bot can be used to allow customers to manage their servers via discord")]
        public bool Enable { get; set; } = false;

        [JsonProperty("Token")]
        [Description("Your discord bot token goes here")]
        [Blur]
        public string Token { get; set; } = "discord token here";

        [JsonProperty("PowerActions")]
        [Description("Enable actions like starting and stopping servers")]
        public bool PowerActions { get; set; } = false;
        
        [JsonProperty("SendCommands")]
        [Description("Allow users to send commands to their servers")]
        public bool SendCommands { get; set; } = false;
    }

    public class DiscordNotificationsData
    {
        [JsonProperty("Enable")]
        [Description("The discord notification system sends you a message everytime a event like a new support chat message is triggered with usefull data describing the event")]
        public bool Enable { get; set; } = false;

        [JsonProperty("WebHook")]
        [Description("The discord webhook the notifications are being sent to")]
        [Blur]
        public string WebHook { get; set; } = "http://your-discord-webhook-url";
    }

    public class DomainsData
    {
        [JsonProperty("Enable")]
        [Description("This enables the domain system")]
        public bool Enable { get; set; } = false;
        
        [JsonProperty("AccountId")]
        [Description("This option specifies the cloudflare account id")]
        public string AccountId { get; set; } = "cloudflare acc id";

        [JsonProperty("Email")]
        [Description("This specifies the cloudflare email to use for communicating with the cloudflare api")]
        public string Email { get; set; } = "cloudflare@acc.email";

        [JsonProperty("Key")]
        [Description("Your cloudflare api key goes here")]
        [Blur]
        public string Key { get; set; } = "secret";
    }

    public class HtmlData
    {
        [JsonProperty("Headers")] public HeadersData Headers { get; set; } = new();
    }

    public class HeadersData
    {
        [JsonProperty("Color")]
        [Description("This specifies the color of the embed generated by platforms like discord when someone posts a link to your moonlight instance")]
        public string Color { get; set; } = "#4b27e8";

        [JsonProperty("Description")]
        [Description("This specifies the description text of the embed generated by platforms like discord when someone posts a link to your moonlight instance and can also help google to index your moonlight instance correctly")]
        public string Description { get; set; } = "the next generation hosting panel";

        [JsonProperty("Keywords")]
        [Description("To help search engines like google to index your moonlight instance correctly you can specify keywords seperated by a comma here")]
        public string Keywords { get; set; } = "moonlight";

        [JsonProperty("Title")]
        [Description("This specifies the title of the embed generated by platforms like discord when someone posts a link to your moonlight instance")]
        public string Title { get; set; } = "Moonlight - endelon.link";
    }

    public class MailData
    {
        [JsonProperty("Email")] public string Email { get; set; } = "username@your.mail.host";

        [JsonProperty("Server")] public string Server { get; set; } = "your.mail.host";

        [JsonProperty("Password")]
        [Blur]
        public string Password { get; set; } = "secret";

        [JsonProperty("Port")] public int Port { get; set; } = 465;

        [JsonProperty("Ssl")] public bool Ssl { get; set; } = true;
    }

    public class MarketingData
    {
        [JsonProperty("BrandName")] public string BrandName { get; set; } = "Endelon Hosting";

        [JsonProperty("Imprint")] public string Imprint { get; set; } = "https://your-site.xyz/imprint";

        [JsonProperty("Privacy")] public string Privacy { get; set; } = "https://your-site.xyz/privacy";
        [JsonProperty("About")] public string About { get; set; } = "https://your-site.xyz/about";
        [JsonProperty("Website")] public string Website { get; set; } = "https://your-site.xyz";
    }

    public class OAuth2Data
    {
        [JsonProperty("OverrideUrl")]
        [Description("This overrides the redirect url which would be typicaly the app url")]
        public string OverrideUrl { get; set; } = "https://only-for-development.cases";

        [JsonProperty("EnableOverrideUrl")]
        [Description("This enables the url override")]
        public bool EnableOverrideUrl { get; set; } = false;

        [JsonProperty("Providers")]
        public OAuth2ProviderData[] Providers { get; set; } = Array.Empty<OAuth2ProviderData>();
    }

    public class OAuth2ProviderData
    {
        [JsonProperty("Id")] public string Id { get; set; }

        [JsonProperty("ClientId")] public string ClientId { get; set; }

        [JsonProperty("ClientSecret")]
        [Blur]
        public string ClientSecret { get; set; }
    }

    public class RatingData
    {
        [JsonProperty("Enabled")]
        [Description("The rating systems shows a user who is registered longer than the set amout of days a popup to rate this platform if he hasnt rated it before")]
        public bool Enabled { get; set; } = false;

        [JsonProperty("Url")]
        [Description("This is the url a user who rated above a set limit is shown to rate you again. Its recommended to put your google or trustpilot rate link here")]
        public string Url { get; set; } = "https://link-to-google-or-smth";

        [JsonProperty("MinRating")]
        [Description("The minimum star count on the rating ranging from 1 to 5")]
        public int MinRating { get; set; } = 4;

        [JsonProperty("DaysSince")]
        [Description("The days a user has to be registered to even be able to get this popup")]
        public int DaysSince { get; set; } = 5;
    }

    public class SecurityData
    {
        [JsonProperty("Token")]
        [Description("This is the moonlight app token. It is used to encrypt and decrypt data and validte tokens and sessions")]
        [Blur]
        public string Token { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("ReCaptcha")] public ReCaptchaData ReCaptcha { get; set; } = new();
    }

    public class ReCaptchaData
    {
        [JsonProperty("Enable")]
        [Description("Enables repatcha at places like the register page. For information how to get your recaptcha credentails go to google.com/recaptcha/about/")]
        public bool Enable { get; set; } = false;

        [JsonProperty("SiteKey")]
        [Blur]
        public string SiteKey { get; set; } = "recaptcha site key here";

        [JsonProperty("SecretKey")]
        [Blur]
        public string SecretKey { get; set; } = "recaptcha secret here";
    }

    public class SentryData
    {
        [JsonProperty("Enable")]
        [Description("Sentry is a way to monitor application crashes and performance issues in real time. Enable this option only if you set a sentry dsn")]
        public bool Enable { get; set; } = false;

        [JsonProperty("Dsn")]
        [Description("The dsn is the key moonlight needs to communicate with your sentry instance")]
        [Blur]
        public string Dsn { get; set; } = "http://your-sentry-url-here";
    }

    public class SmartDeployData
    {
        [JsonProperty("Server")] public SmartDeployServerData Server { get; set; } = new();
    }

    public class SmartDeployServerData
    {
        [JsonProperty("EnableOverride")] public bool EnableOverride { get; set; } = false;

        [JsonProperty("OverrideNode")] public long OverrideNode { get; set; } = 1;
    }

    public class StatisticsData
    {
        [JsonProperty("Enabled")] public bool Enabled { get; set; } = false;

        [JsonProperty("Wait")] public long Wait { get; set; } = 15;
    }

    public class SubscriptionsData
    {
        [JsonProperty("SellPass")] public SellPassData SellPass { get; set; } = new();
    }

    public class SellPassData
    {
        [JsonProperty("Enable")] public bool Enable { get; set; } = false;

        [JsonProperty("Url")] public string Url { get; set; } = "https://not-implemented-yet";
    }
}