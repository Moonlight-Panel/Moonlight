namespace Moonlight.App.Configuration;

using System;
using Newtonsoft.Json;

public class ConfigV1
{
    [JsonProperty("Moonlight")] public MoonlightData Moonlight { get; set; } = new();

    public class MoonlightData
    {
        [JsonProperty("AppUrl")] public string AppUrl { get; set; } = "http://your-moonlight-url-without-slash";

        [JsonProperty("Database")] public DatabaseData Database { get; set; } = new();

        [JsonProperty("DiscordBotApi")] public DiscordBotData DiscordBotApi { get; set; } = new();

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

        [JsonProperty("Stripe")] public StripeData Stripe { get; set; } = new();
    }
    
    public class StripeData
    {
        [JsonProperty("ApiKey")] public string ApiKey { get; set; } = "";
    }

    public class CleanupData
    {
        [JsonProperty("Cpu")] public long Cpu { get; set; } = 90;

        [JsonProperty("Memory")] public long Memory { get; set; } = 8192;

        [JsonProperty("Wait")] public long Wait { get; set; } = 15;

        [JsonProperty("Uptime")] public long Uptime { get; set; } = 6;

        [JsonProperty("Enable")] public bool Enable { get; set; } = false;

        [JsonProperty("MinUptime")] public long MinUptime { get; set; } = 10;
    }

    public class DatabaseData
    {
        [JsonProperty("Database")] public string Database { get; set; } = "moonlight_db";

        [JsonProperty("Host")] public string Host { get; set; } = "your.database.host";

        [JsonProperty("Password")] public string Password { get; set; } = "secret";

        [JsonProperty("Port")] public long Port { get; set; } = 3306;

        [JsonProperty("Username")] public string Username { get; set; } = "moonlight_user";
    }

    public class DiscordBotData
    {
        [JsonProperty("Enable")] public bool Enable { get; set; } = false;

        [JsonProperty("Token")] public string Token { get; set; } = "discord token here";

        [JsonProperty("PowerActions")] public bool PowerActions { get; set; } = false;
        [JsonProperty("SendCommands")] public bool SendCommands { get; set; } = false;
    }

    public class DiscordNotificationsData
    {
        [JsonProperty("Enable")] public bool Enable { get; set; } = false;

        [JsonProperty("WebHook")] public string WebHook { get; set; } = "http://your-discord-webhook-url";
    }

    public class DomainsData
    {
        [JsonProperty("Enable")] public bool Enable { get; set; } = false;
        [JsonProperty("AccountId")] public string AccountId { get; set; } = "cloudflare acc id";

        [JsonProperty("Email")] public string Email { get; set; } = "cloudflare@acc.email";

        [JsonProperty("Key")] public string Key { get; set; } = "secret";
    }

    public class HtmlData
    {
        [JsonProperty("Headers")] public HeadersData Headers { get; set; } = new();
    }

    public class HeadersData
    {
        [JsonProperty("Color")] public string Color { get; set; } = "#4b27e8";

        [JsonProperty("Description")] public string Description { get; set; } = "the next generation hosting panel";

        [JsonProperty("Keywords")] public string Keywords { get; set; } = "moonlight";

        [JsonProperty("Title")] public string Title { get; set; } = "Moonlight - endelon.link";
    }

    public class MailData
    {
        [JsonProperty("Email")] public string Email { get; set; } = "username@your.mail.host";

        [JsonProperty("Server")] public string Server { get; set; } = "your.mail.host";

        [JsonProperty("Password")] public string Password { get; set; } = "secret";

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
        [JsonProperty("OverrideUrl")] public string OverrideUrl { get; set; } = "https://only-for-development.cases";

        [JsonProperty("EnableOverrideUrl")] public bool EnableOverrideUrl { get; set; } = false;

        [JsonProperty("Providers")]
        public OAuth2ProviderData[] Providers { get; set; } = Array.Empty<OAuth2ProviderData>();
    }

    public class OAuth2ProviderData
    {
        [JsonProperty("Id")] public string Id { get; set; }

        [JsonProperty("ClientId")] public string ClientId { get; set; }

        [JsonProperty("ClientSecret")] public string ClientSecret { get; set; }
    }

    public class RatingData
    {
        [JsonProperty("Enabled")] public bool Enabled { get; set; } = false;

        [JsonProperty("Url")] public string Url { get; set; } = "https://link-to-google-or-smth";

        [JsonProperty("MinRating")] public int MinRating { get; set; } = 4;

        [JsonProperty("DaysSince")] public int DaysSince { get; set; } = 5;
    }

    public class SecurityData
    {
        [JsonProperty("Token")] public string Token { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("ReCaptcha")] public ReCaptchaData ReCaptcha { get; set; } = new();
    }

    public class ReCaptchaData
    {
        [JsonProperty("Enable")] public bool Enable { get; set; } = false;

        [JsonProperty("SiteKey")] public string SiteKey { get; set; } = "recaptcha site key here";

        [JsonProperty("SecretKey")] public string SecretKey { get; set; } = "recaptcha secret here";
    }

    public class SentryData
    {
        [JsonProperty("Enable")] public bool Enable { get; set; } = false;

        [JsonProperty("Dsn")] public string Dsn { get; set; } = "http://your-sentry-url-here";
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