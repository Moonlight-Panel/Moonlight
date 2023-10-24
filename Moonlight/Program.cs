using BlazorTable;
using Moonlight.App.Actions.Dummy;
using Moonlight.App.Database;
using Moonlight.App.Database.Enums;
using Moonlight.App.Extensions;
using Moonlight.App.Helpers;
using Moonlight.App.Helpers.LogMigrator;
using Moonlight.App.Repositories;
using Moonlight.App.Services;
using Moonlight.App.Services.Background;
using Moonlight.App.Services.Interop;
using Moonlight.App.Services.ServiceManage;
using Moonlight.App.Services.Store;
using Moonlight.App.Services.Users;
using Moonlight.App.Services.Utils;
using Serilog;

Directory.CreateDirectory(PathBuilder.Dir("storage"));
Directory.CreateDirectory(PathBuilder.Dir("storage", "logs"));

var logConfig = new LoggerConfiguration();

logConfig = logConfig.Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate:
        "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}");

Log.Logger = logConfig.CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Init plugin system
var pluginService = new PluginService();
builder.Services.AddSingleton(pluginService);

await pluginService.Load(builder);
await pluginService.RunPreInit();

builder.Services.AddDbContext<DataContext>();

// Repositories
builder.Services.AddScoped(typeof(Repository<>));

// Services / Utils
builder.Services.AddScoped<JwtService>();

// Services / Interop
builder.Services.AddScoped<CookieService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<ModalService>();
builder.Services.AddScoped<AlertService>();

// Services / Store
builder.Services.AddScoped<StoreService>();
builder.Services.AddScoped<StoreAdminService>();
builder.Services.AddScoped<StoreOrderService>();
builder.Services.AddScoped<StoreGiftService>();
builder.Services.AddSingleton<StorePaymentService>();
builder.Services.AddScoped<TransactionService>();

// Services / Users
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserAuthService>();
builder.Services.AddScoped<UserDetailsService>();

// Services / Background
builder.Services.AddSingleton<AutoMailSendService>();

// Services / ServiceManage
builder.Services.AddScoped<ServiceService>();
builder.Services.AddSingleton<ServiceAdminService>();

// Services
builder.Services.AddScoped<IdentityService>();
builder.Services.AddSingleton<ConfigService>();
builder.Services.AddSingleton<SessionService>();
builder.Services.AddSingleton<BucketService>();
builder.Services.AddSingleton<MailService>();

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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers();

// Auto start background services
app.Services.GetRequiredService<AutoMailSendService>();

var serviceService = app.Services.GetRequiredService<ServiceAdminService>();
await serviceService.RegisterAction(ServiceType.Server, new DummyActions());

await pluginService.RunPrePost(app);

app.Run();