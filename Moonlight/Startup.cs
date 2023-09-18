using BlazorDownloadFile;
using BlazorTable;
using HealthChecks.UI.Client;
using Moonlight.App.ApiClients.CloudPanel;
using Moonlight.App.ApiClients.Daemon;
using Moonlight.App.ApiClients.Modrinth;
using Moonlight.App.ApiClients.Paper;
using Moonlight.App.ApiClients.Telemetry;
using Moonlight.App.ApiClients.Wings;
using Moonlight.App.Database;
using Moonlight.App.Diagnostics.HealthChecks;
using Moonlight.App.Events;
using Moonlight.App.Helpers;
using Moonlight.App.Helpers.Wings;
using Moonlight.App.LogMigrator;
using Moonlight.App.Repositories;
using Moonlight.App.Repositories.Domains;
using Moonlight.App.Repositories.Servers;
using Moonlight.App.Services;
using Moonlight.App.Services.Background;
using Moonlight.App.Services.DiscordBot;
using Moonlight.App.Services.Files;
using Moonlight.App.Services.Interop;
using Moonlight.App.Services.Mail;
using Moonlight.App.Services.Notifications;
using Moonlight.App.Services.Plugins;
using Moonlight.App.Services.Sessions;
using Moonlight.App.Services.Statistics;
using Moonlight.App.Services.Tickets;
using Sentry;
using Serilog;
using Serilog.Events;
using Stripe;
using SubscriptionService = Moonlight.App.Services.SubscriptionService;

namespace Moonlight;

public class Startup
{
    // Base services

    private StorageService StorageService;
    private ConfigService ConfigService;
    private DatabaseCheckupService DatabaseCheckupService;
    private EventSystem Event;
    private LetsEncryptService LetsEncryptService;
    private PluginService PluginService;
    
    // Builders
    private WebApplicationBuilder WebApplicationBuilder;
    private WebApplication WebApplication;

    public async Task Init(string[] args)
    {
        WebApplicationBuilder = WebApplication.CreateBuilder(args);

        await PreInitServices();
        await SetupLoggers();
        Logger.Info("[1/5] Initialized base services and loggers");
        await PreInit();
        await Configure();
        await BuildServices();
        
        WebApplication = WebApplicationBuilder.Build();

        await ConfigurePipeline();
        Logger.Info("Initialisation complete!");
    }

    public async Task Start()
    {
        Logger.Info("Starting moonlight panel");
        
        await StartBackgroundServices();
        await PostInit();
        
        Logger.Info("Done. Going live now!");

        if (!Uri.TryCreate(ConfigService.Get().Moonlight.AppUrl, UriKind.RelativeOrAbsolute, out Uri uri))
        {
            Logger.Fatal("Invalid app url");
            return;
        }
        
        if(ConfigService.DebugMode || uri.HostNameType == UriHostNameType.IPv4 || !ConfigService.Get().Moonlight.LetsEncrypt.Enable)
            await WebApplication.RunAsync();
        else
            await WebApplication.RunAsync(ConfigService.Get().Moonlight.AppUrl);
    }
    
    private Task PreInitServices()
    {
        StorageService = new();
        ConfigService = new(StorageService);
        DatabaseCheckupService = new(ConfigService);
        Event = new();
        LetsEncryptService = new(ConfigService, Event);
        PluginService = new();

        return Task.CompletedTask;
    }
    private Task StartBackgroundServices()
    {
        Logger.Info("[1/2] Starting background services");
        
        _ = WebApplication.Services.GetRequiredService<CleanupService>();
        _ = WebApplication.Services.GetRequiredService<DiscordBotService>();
        _ = WebApplication.Services.GetRequiredService<StatisticsCaptureService>();
        _ = WebApplication.Services.GetRequiredService<DiscordNotificationService>();
        _ = WebApplication.Services.GetRequiredService<MalwareBackgroundScanService>();
        _ = WebApplication.Services.GetRequiredService<TelemetryService>();
        _ = WebApplication.Services.GetRequiredService<TempMailService>();
        _ = WebApplication.Services.GetRequiredService<DdosProtectionService>();
        _ = WebApplication.Services.GetRequiredService<MoonlightService>();
        
        return Task.CompletedTask;
    }
    private Task PostInit()
    {
        Logger.Info("[2/2] Started post init tasks");
        
        Task.Run(async () =>
        {
            await LetsEncryptService.AutoProcess();
        });
        
        return Task.CompletedTask;
    }
    private async Task PreInit()
    {
        Logger.Info("[2/5] Running pre init tasks");
        
        await StorageService.EnsureCreated();
        await DatabaseCheckupService.Perform();
        await PluginService.ReloadPlugins();
    }
    private Task Configure()
    {
        Logger.Info("[3/5] Configuring kestrel");
        
        WebApplicationBuilder.WebHost.ConfigureKestrel(options =>
        {
            options.ConfigureHttpsDefaults(httpsOptions =>
            {
                httpsOptions.ServerCertificateSelector = LetsEncryptService.SelectCertificate;
            }); 
        });
        
        WebApplicationBuilder.Services.AddServerSideBlazor()
            .AddHubOptions(options =>
            {
                options.MaximumReceiveMessageSize = 10000000;
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                options.HandshakeTimeout = TimeSpan.FromSeconds(10);
            });
        
        WebApplicationBuilder.Services.AddHttpContextAccessor();
        WebApplicationBuilder.Services.AddRazorPages();
        
        WebApplicationBuilder.Services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("Database")
            .AddCheck<NodeHealthCheck>("Nodes")
            .AddCheck<DaemonHealthCheck>("Daemons");

        if (ConfigService.Get().Moonlight.Sentry.Enable)
        {
            WebApplicationBuilder.WebHost.UseSentry(options =>
            {
                options.Dsn = ConfigService
                    .Get()
                    .Moonlight.Sentry.Dsn;

                options.Debug = ConfigService.DebugMode;
                options.DiagnosticLevel = SentryLevel.Warning;
                options.TracesSampleRate = 1.0;

                options.DiagnosticLogger = new SentryDiagnosticsLogger(SentryLevel.Warning);
            });
        }
        
        StripeConfiguration.ApiKey = ConfigService
            .Get()
            .Moonlight.Stripe.ApiKey;
        
        return Task.CompletedTask;
    }
    private async Task BuildServices()
    {
        Logger.Info("[4/5] Building services");
        
        await BuildSingletonServices();
        await BuildScopedServices();
        
        // Third party services
        WebApplicationBuilder.Services.AddBlazorTable();
        WebApplicationBuilder.Services.AddBlazorContextMenu();
        WebApplicationBuilder.Services.AddBlazorDownloadFile();

        await PluginService.BuildServices(WebApplicationBuilder.Services);
    }
    private Task BuildScopedServices()
    {
        // Databases
        WebApplicationBuilder.Services.AddDbContext<DataContext>();
        
        // Repositories
        WebApplicationBuilder.Services.AddScoped<UserRepository>();
        WebApplicationBuilder.Services.AddScoped<NodeRepository>();
        WebApplicationBuilder.Services.AddScoped<ServerRepository>();
        WebApplicationBuilder.Services.AddScoped<ServerBackupRepository>();
        WebApplicationBuilder.Services.AddScoped<ImageRepository>();
        WebApplicationBuilder.Services.AddScoped<DomainRepository>();
        WebApplicationBuilder.Services.AddScoped<SharedDomainRepository>();
        WebApplicationBuilder.Services.AddScoped<RevokeRepository>();
        WebApplicationBuilder.Services.AddScoped<NotificationRepository>();
        WebApplicationBuilder.Services.AddScoped<DdosAttackRepository>();
        WebApplicationBuilder.Services.AddScoped<SubscriptionRepository>();
        WebApplicationBuilder.Services.AddScoped<LoadingMessageRepository>();
        WebApplicationBuilder.Services.AddScoped<NewsEntryRepository>();
        WebApplicationBuilder.Services.AddScoped<NodeAllocationRepository>();
        WebApplicationBuilder.Services.AddScoped<StatisticsRepository>();
        WebApplicationBuilder.Services.AddScoped(typeof(Repository<>));
        
        // Js interopt and session stuff
        WebApplicationBuilder.Services.AddScoped<CookieService>();
        WebApplicationBuilder.Services.AddScoped<IdentityService>();
        WebApplicationBuilder.Services.AddScoped<IpLocateService>();
        WebApplicationBuilder.Services.AddScoped<AlertService>();
        WebApplicationBuilder.Services.AddScoped<ClipboardService>();
        WebApplicationBuilder.Services.AddScoped<ModalService>();
        WebApplicationBuilder.Services.AddScoped<FileDownloadService>();
        WebApplicationBuilder.Services.AddScoped<KeyListenerService>();
        WebApplicationBuilder.Services.AddScoped<PopupService>();
        WebApplicationBuilder.Services.AddScoped<SessionClientService>();
        WebApplicationBuilder.Services.AddScoped<ReCaptchaService>();
        WebApplicationBuilder.Services.AddScoped<DynamicBackgroundService>();
        
        // Main application stuff
        WebApplicationBuilder.Services.AddScoped<SmartTranslateService>();
        WebApplicationBuilder.Services.AddScoped<UserService>();
        WebApplicationBuilder.Services.AddScoped<TotpService>();
        WebApplicationBuilder.Services.AddScoped<ToastService>();
        WebApplicationBuilder.Services.AddScoped<NodeService>();
        WebApplicationBuilder.Services.AddScoped<ServerService>();
        WebApplicationBuilder.Services.AddScoped<DomainService>();
        WebApplicationBuilder.Services.AddScoped<OneTimeJwtService>();
        WebApplicationBuilder.Services.AddScoped<NotificationAdminService>();
        WebApplicationBuilder.Services.AddScoped<SmartDeployService>();
        WebApplicationBuilder.Services.AddScoped<WebSpaceService>();
        WebApplicationBuilder.Services.AddScoped<StatisticsViewService>();
        WebApplicationBuilder.Services.AddScoped<RatingService>();
        WebApplicationBuilder.Services.AddScoped<IpBanService>();
        WebApplicationBuilder.Services.AddScoped<SubscriptionService>();
        WebApplicationBuilder.Services.AddScoped<BillingService>();
        WebApplicationBuilder.Services.AddScoped<TicketClientService>();
        WebApplicationBuilder.Services.AddScoped<TicketAdminService>();
        WebApplicationBuilder.Services.AddScoped<MalwareScanService>();
        WebApplicationBuilder.Services.AddScoped<MailService>();
        WebApplicationBuilder.Services.AddScoped<WingsServerConverter>();
        WebApplicationBuilder.Services.AddScoped<WingsConsoleHelper>();

        return Task.CompletedTask;
    }
    private Task BuildSingletonServices()
    {
        WebApplicationBuilder.Services.AddSingleton(ConfigService);
        WebApplicationBuilder.Services.AddSingleton(Event);
        WebApplicationBuilder.Services.AddSingleton(PluginService);
        WebApplicationBuilder.Services.AddSingleton<StorageService>();
        
        // Api helpers
        WebApplicationBuilder.Services.AddSingleton<PaperApiHelper>();
        WebApplicationBuilder.Services.AddSingleton<DaemonApiHelper>();
        WebApplicationBuilder.Services.AddSingleton<CloudPanelApiHelper>();
        WebApplicationBuilder.Services.AddSingleton<ModrinthApiHelper>();
        WebApplicationBuilder.Services.AddSingleton<TelemetryApiHelper>();
        WebApplicationBuilder.Services.AddSingleton<WingsApiHelper>();

        // Main application stuff
        WebApplicationBuilder.Services.AddSingleton<HostSystemHelper>();
        WebApplicationBuilder.Services.AddSingleton<OAuth2Service>();
        WebApplicationBuilder.Services.AddSingleton<PluginStoreService>();
        WebApplicationBuilder.Services.AddSingleton<TicketServerService>();
        WebApplicationBuilder.Services.AddSingleton<IpVerificationService>();
        WebApplicationBuilder.Services.AddSingleton<NotificationServerService>();
        WebApplicationBuilder.Services.AddSingleton<DateTimeService>();
        WebApplicationBuilder.Services.AddSingleton<MoonlightService>();
        WebApplicationBuilder.Services.AddSingleton<ResourceService>();
        WebApplicationBuilder.Services.AddSingleton<BucketService>();
        
        // Helpers
        WebApplicationBuilder.Services.AddSingleton<SmartTranslateHelper>();
        WebApplicationBuilder.Services.AddSingleton<WingsJwtHelper>();
        
        // Background services
        WebApplicationBuilder.Services.AddSingleton<DiscordBotService>();
        WebApplicationBuilder.Services.AddSingleton<StatisticsCaptureService>();
        WebApplicationBuilder.Services.AddSingleton<DiscordNotificationService>();
        WebApplicationBuilder.Services.AddSingleton<CleanupService>();
        WebApplicationBuilder.Services.AddSingleton<MalwareBackgroundScanService>();
        WebApplicationBuilder.Services.AddSingleton<TelemetryService>();
        WebApplicationBuilder.Services.AddSingleton<TempMailService>();
        WebApplicationBuilder.Services.AddSingleton<DdosProtectionService>();
        WebApplicationBuilder.Services.AddSingleton<SessionServerService>();

        return Task.CompletedTask;
    }
    private Task SetupLoggers()
    {
        var logConfig = new LoggerConfiguration();

        if (ConfigService.DebugMode)
            logConfig = logConfig.MinimumLevel.Verbose();
        else
            logConfig = logConfig.MinimumLevel.Information();

        if (ConfigService.Get().Moonlight.Sentry.Enable)
        {
            if (ConfigService.DebugMode)
            {
                logConfig = logConfig.WriteTo.Sentry(options =>
                {
                    options.MinimumBreadcrumbLevel = LogEventLevel.Debug;
                    options.MinimumEventLevel = LogEventLevel.Warning;
                });
            }
            else
            {
                logConfig = logConfig.WriteTo.Sentry(options =>
                {
                    options.MinimumBreadcrumbLevel = LogEventLevel.Information;
                    options.MinimumEventLevel = LogEventLevel.Warning;
                });
            }
        }

        logConfig = logConfig.Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate:
                "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(PathBuilder.File("storage", "logs", $"{DateTime.UtcNow:yyyy-MM-dd}.log"));

        Log.Logger = logConfig.CreateLogger();
        
        WebApplicationBuilder.Logging.ClearProviders();
        WebApplicationBuilder.Logging.AddProvider(new LogMigratorProvider());
        
        return Task.CompletedTask;
    }
    private Task ConfigurePipeline()
    {
        Logger.Info("[5/5] Configuring pipeline");
        
        if (!WebApplication.Environment.IsDevelopment())
        {
            WebApplication.UseExceptionHandler("/Error");
        }
        
        // Sentry
        if (ConfigService.Get().Moonlight.Sentry.Enable)
        {
            WebApplication.UseSentryTracing();
        }

        WebApplication.UseStaticFiles();
        WebApplication.UseRouting();
        WebApplication.UseWebSockets();

        WebApplication.MapControllers();

        WebApplication.MapBlazorHub();
        WebApplication.MapFallbackToPage("/_Host");
        WebApplication.MapHealthChecks("/_health", new()
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        
        return Task.CompletedTask;
    }
}