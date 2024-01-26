using BlazorTable;
using Moonlight.Core.Database;
using Moonlight.Core.Actions.Dummy;
using Moonlight.Core.Database;
using Moonlight.Core.Extensions;
using Moonlight.Core.Helpers;
using Moonlight.Core.Helpers.LogMigrator;
using Moonlight.Core.Repositories;
using Moonlight.Core.Services;
using Moonlight.Core.Services.Background;
using Moonlight.Core.Services.Interop;
using Moonlight.Core.Services.Users;
using Moonlight.Core.Services.Utils;
using Moonlight.Features.Advertisement.Services;
using Moonlight.Features.Community.Services;
using Moonlight.Features.Servers.Http.Middleware;
using Moonlight.Features.Servers.Services;
using Moonlight.Features.ServiceManagement.Entities.Enums;
using Moonlight.Features.ServiceManagement.Services;
using Moonlight.Features.StoreSystem.Services;
using Moonlight.Features.Theming.Services;
using Moonlight.Features.Ticketing.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var configService = new ConfigService();

Directory.CreateDirectory(PathBuilder.Dir("storage"));
Directory.CreateDirectory(PathBuilder.Dir("storage", "logs"));

var logConfig = new LoggerConfiguration();
var now = DateTime.UtcNow;
var logPath = PathBuilder.File("storage", "logs", $"moonlight-{now.Day}-{now.Month}-{now.Year}---{now.Hour}-{now.Minute}.log");

logConfig = logConfig.Enrich.FromLogContext()
    .WriteTo.File(logPath, outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
    .WriteTo.Console(
        outputTemplate:
        "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}");

if (args.Contains("--debug") || builder.Environment.IsDevelopment())
    logConfig = logConfig.MinimumLevel.Debug();
else
    logConfig = logConfig.MinimumLevel.Information();

Log.Logger = logConfig.CreateLogger();

// Init plugin system
var pluginService = new PluginService();
builder.Services.AddSingleton(pluginService);

await pluginService.Load(builder);
await pluginService.RunPreInit();

// TODO: Add automatic assembly scanning
// dependency injection registration
// using attributes

builder.Services.AddDbContext<DataContext>();

// Repositories
builder.Services.AddScoped(typeof(Repository<>));

// Services / Utils
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<ConnectionService>();

// Services / Interop
builder.Services.AddScoped<CookieService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<ModalService>();
builder.Services.AddScoped<AlertService>();
builder.Services.AddScoped<FileDownloadService>();
builder.Services.AddScoped<AdBlockService>();

// Services / Store
builder.Services.AddScoped<StoreService>();
builder.Services.AddScoped<StoreAdminService>();
builder.Services.AddScoped<StoreOrderService>();
builder.Services.AddScoped<StoreGiftService>();
builder.Services.AddSingleton<StorePaymentService>();
builder.Services.AddScoped<TransactionService>();

// Services / Community
builder.Services.AddScoped<PostService>();

// Services / Users
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserAuthService>();
builder.Services.AddScoped<UserDetailsService>();
builder.Services.AddScoped<UserDeleteService>();

// Services / Background
builder.Services.AddSingleton<AutoMailSendService>();

// Services / ServiceManage
builder.Services.AddScoped<ServiceService>();
builder.Services.AddSingleton<ServiceAdminService>();
builder.Services.AddSingleton<ServiceDefinitionService>();
builder.Services.AddSingleton<ServiceManageService>();

// Services / Ticketing
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<TicketChatService>();
builder.Services.AddScoped<TicketCreateService>();

// Services / Servers
builder.Services.AddSingleton<NodeService>();

// Services
builder.Services.AddScoped<IdentityService>();
builder.Services.AddSingleton(configService);
builder.Services.AddSingleton<SessionService>();
builder.Services.AddSingleton<BucketService>();
builder.Services.AddSingleton<MailService>();
builder.Services.AddSingleton<MoonlightService>();
builder.Services.AddSingleton<ThemeService>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddBlazorTable();

builder.Logging.ClearProviders();
builder.Logging.AddProvider(new LogMigrateProvider());

var config =
    new ConfigurationBuilder().AddJsonString(
        "{\"LogLevel\":{\"Default\":\"Information\",\"Microsoft.AspNetCore\":\"Warning\"}}");
builder.Logging.AddConfiguration(config.Build());

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseMiddleware<NodeMiddleware>();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers();

// Auto start background services
app.Services.GetRequiredService<AutoMailSendService>();

var moonlightService = app.Services.GetRequiredService<MoonlightService>();
moonlightService.Application = app;
moonlightService.LogPath = logPath;

var serviceService = app.Services.GetRequiredService<ServiceDefinitionService>();
serviceService.Register<DummyServiceDefinition>(ServiceType.Server);

await pluginService.RunPrePost(app);

app.Run();