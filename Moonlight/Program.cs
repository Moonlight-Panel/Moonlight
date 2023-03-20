using BlazorDownloadFile;
using BlazorTable;
using CurrieTechnologies.Razor.SweetAlert2;
using Logging.Net;
using Moonlight.App.Database;
using Moonlight.App.Helpers;
using Moonlight.App.LogMigrator;
using Moonlight.App.Repositories;
using Moonlight.App.Repositories.Domains;
using Moonlight.App.Repositories.LogEntries;
using Moonlight.App.Repositories.Servers;
using Moonlight.App.Repositories.Subscriptions;
using Moonlight.App.Services;
using Moonlight.App.Services.DiscordBot;
using Moonlight.App.Services.Interop;
using Moonlight.App.Services.LogServices;
using Moonlight.App.Services.Notifications;
using Moonlight.App.Services.OAuth2;
using Moonlight.App.Services.Sessions;
using Moonlight.App.Services.Support;

namespace Moonlight
{
    public class Program
    {
        // App version. Change for release
        public static readonly string AppVersion = $"InDev {Formatter.FormatDateOnly(DateTime.Now.Date)}";
        
        public static void Main(string[] args)
        {
            Logger.UsedLogger = new CacheLogger();
            
            Logger.Info($"Working dir: {Directory.GetCurrentDirectory()}");

            var builder = WebApplication.CreateBuilder(args);
            
            // Switch to logging.net injection
            // TODO: Enable in production
            //builder.Logging.ClearProviders();
            //builder.Logging.AddProvider(new LogMigratorProvider());

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddHttpContextAccessor();
            
            // Databases
            builder.Services.AddDbContext<DataContext>();
            
            // Repositories
            builder.Services.AddSingleton<SessionRepository>();
            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<NodeRepository>();
            builder.Services.AddScoped<ServerRepository>();
            builder.Services.AddScoped<ServerBackupRepository>();
            builder.Services.AddScoped<DatabaseRepository>();
            builder.Services.AddScoped<ImageRepository>();
            builder.Services.AddScoped<SupportMessageRepository>();
            builder.Services.AddScoped<DomainRepository>();
            builder.Services.AddScoped<SharedDomainRepository>();
            builder.Services.AddScoped<SubscriptionRepository>();
            builder.Services.AddScoped<SubscriptionLimitRepository>();
            builder.Services.AddScoped<RevokeRepository>();
            builder.Services.AddScoped<NotificationRepository>();
            builder.Services.AddScoped<AaPanelRepository>();
            builder.Services.AddScoped<WebsiteRepository>();

            builder.Services.AddScoped<AuditLogEntryRepository>();
            builder.Services.AddScoped<ErrorLogEntryRepository>();
            builder.Services.AddScoped<SecurityLogEntryRepository>();
            
            // Services
            builder.Services.AddSingleton<ConfigService>();
            builder.Services.AddScoped<CookieService>();
            builder.Services.AddScoped<IdentityService>();
            builder.Services.AddScoped<IpLocateService>();
            builder.Services.AddScoped<SessionService>();
            builder.Services.AddScoped<AlertService>();
            builder.Services.AddScoped<SmartTranslateService>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<TotpService>();
            builder.Services.AddScoped<ToastService>();
            builder.Services.AddScoped<NodeService>();
            builder.Services.AddSingleton<MessageService>();
            builder.Services.AddScoped<ServerService>();
            builder.Services.AddSingleton<PaperService>();
            builder.Services.AddScoped<ClipboardService>();
            builder.Services.AddSingleton<ResourceService>();
            builder.Services.AddScoped<DomainService>();
            builder.Services.AddScoped<OneTimeJwtService>();
            builder.Services.AddScoped<SubscriptionService>();
            builder.Services.AddSingleton<NotificationServerService>();
            builder.Services.AddScoped<NotificationAdminService>();
            builder.Services.AddScoped<NotificationClientService>();
            
            builder.Services.AddScoped<GoogleOAuth2Service>();
            builder.Services.AddScoped<DiscordOAuth2Service>();

            // Loggers
            builder.Services.AddScoped<SecurityLogService>();
            builder.Services.AddScoped<AuditLogService>();
            builder.Services.AddScoped<ErrorLogService>();
            builder.Services.AddScoped<LogService>();
            builder.Services.AddScoped<MailService>();
            builder.Services.AddSingleton<TrashMailDetectorService>();
            builder.Services.AddScoped<WebsiteService>();

            // Support
            builder.Services.AddSingleton<SupportServerService>();
            builder.Services.AddScoped<SupportAdminService>();
            builder.Services.AddScoped<SupportClientService>();

            // Helpers
            builder.Services.AddSingleton<SmartTranslateHelper>();
            builder.Services.AddScoped<WingsApiHelper>();
            builder.Services.AddScoped<WingsServerConverter>();
            builder.Services.AddSingleton<WingsJwtHelper>();
            builder.Services.AddScoped<WingsConsoleHelper>();
            builder.Services.AddSingleton<PaperApiHelper>();
            builder.Services.AddSingleton<HostSystemHelper>();
            builder.Services.AddScoped<DaemonApiHelper>();
            
            // Background services
            builder.Services.AddSingleton<DiscordBotService>();

            // Third party services

            builder.Services.AddBlazorTable();
            builder.Services.AddSweetAlert2(options => { options.Theme = SweetAlertTheme.Dark; });
            builder.Services.AddBlazorContextMenu();
            builder.Services.AddBlazorDownloadFile();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseWebSockets();

            app.MapControllers();
            
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");
            
            // Support service
            var supportServerService = app.Services.GetRequiredService<SupportServerService>();
            
            // Discord bot service
            //var discordBotService = app.Services.GetRequiredService<DiscordBotService>();

            app.Run();
        }
    }
}