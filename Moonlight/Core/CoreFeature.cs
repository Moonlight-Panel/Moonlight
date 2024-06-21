using BlazorTable;
using Microsoft.AspNetCore.Components;
using MoonCore.Abstractions;
using MoonCore.Helpers;
using MoonCore.Services;
using MoonCoreUI.Extensions;
using MoonCoreUI.Services;
using Moonlight.Core.Configuration;
using Moonlight.Core.Database;
using Moonlight.Core.Database.Entities;
using Moonlight.Core.Implementations.Diagnose;
using Moonlight.Core.Implementations.UI.Admin.AdminColumns;
using Moonlight.Core.Implementations.UI.Index;
using Moonlight.Core.Interfaces;
using Moonlight.Core.Interfaces.Ui.Admin;
using Moonlight.Core.Interfaces.UI.User;
using Moonlight.Core.Models;
using Moonlight.Core.Models.Abstractions;
using Moonlight.Core.Models.Abstractions.Feature;
using Moonlight.Core.Models.Enums;
using Moonlight.Core.Repositories;
using Moonlight.Core.Services;
using Microsoft.OpenApi.Models;
using Moonlight.Core.Attributes;
using Moonlight.Core.Http.Middleware;
using Moonlight.Core.Implementations.ApiDefinition;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Moonlight.Core;

public class CoreFeature : MoonlightFeature
{
    public CoreFeature()
    {
        Name = "Moonlight Core";
        Author = "MasuOwO and contributors";
        IssueTracker = "https://github.com/Moonlight-Panel/Moonlight/issues";
    }

    public override Task OnPreInitialized(PreInitContext context)
    {
        // Load configuration
        var configService = new ConfigService<CoreConfiguration>(
            PathBuilder.File("storage", "configs", "core.json")
        );

        var config = configService.Get();

        // Services
        context.EnableDependencyInjection<CoreFeature>();

        var builder = context.Builder;

        builder.Services.AddDbContext<DataContext>();

        // 
        builder.Services.AddSingleton(new JwtService<CoreJwtType>(config.Security.Token));

        // Mooncore services
        builder.Services.AddScoped(typeof(Repository<>), typeof(GenericRepository<>));
        builder.Services.AddScoped<CookieService>();
        builder.Services.AddScoped<FileDownloadService>();
        builder.Services.AddScoped<AlertService>();
        builder.Services.AddScoped<ToastService>();
        builder.Services.AddScoped<ClipboardService>();
        builder.Services.AddScoped<ModalService>();

        builder.Services.AddMoonCoreUi(configuration =>
        {
            configuration.ToastJavascriptPrefix = "moonlight.toasts";
            configuration.ModalJavascriptPrefix = "moonlight.modals";
            configuration.AlertJavascriptPrefix = "moonlight.alerts";
            configuration.ClipboardJavascriptPrefix = "moonlight.clipboard";
            configuration.FileDownloadJavascriptPrefix = "moonlight.utils";
        });

        // Add external services and blazor/asp.net stuff
        builder.Services.AddRazorPages();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddControllers();
        builder.Services.AddBlazorTable();

        // Configure blazor pipeline in detail
        builder.Services.AddServerSideBlazor().AddHubOptions(options =>
        {
            options.MaximumReceiveMessageSize = ByteSizeValue.FromKiloBytes(config.Http.MessageSizeLimit).Bytes;
        });

        // Setup authentication if required
        if (config.Authentication.UseDefaultAuthentication)
            builder.Services.AddScoped<IAuthenticationProvider, DefaultAuthenticationProvider>();

        // Setup http upload limit
        context.Builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = ByteSizeValue.FromMegaBytes(config.Http.UploadLimit).Bytes;
        });

        // Assets

        // - Javascript
        context.AddAsset("Core", "js/bootstrap.js");
        context.AddAsset("Core", "js/moonlight.js");
        context.AddAsset("Core", "js/sweetalert2.js");
        context.AddAsset("Core", "js/toaster.js");
        context.AddAsset("Core", "js/sidebar.js");
        context.AddAsset("Core", "js/alerter.js");

        // - Css
        context.AddAsset("Core", "css/blazor.css");
        context.AddAsset("Core", "css/boxicons.css");
        context.AddAsset("Core", "css/sweetalert2dark.css");
        context.AddAsset("Core", "css/utils.css");

        // Api
        if (config.Development.EnableApiReference)
        {
            builder.Services.AddSwaggerGen(async options =>
            {
                foreach (var definition in await context.Plugins.GetImplementations<IApiDefinition>())
                {
                    options.SwaggerDoc(
                        definition.GetId(),
                        new OpenApiInfo()
                        {
                            Title = definition.GetName(),
                            Version = definition.GetVersion()
                        }
                    );
                }

                options.SwaggerGeneratorOptions.DocInclusionPredicate = (document, description) =>
                {
                    foreach (var attribute in description.CustomAttributes())
                    {
                        if (attribute is ApiDocumentAttribute documentAttribute)
                            return document == documentAttribute.Name;
                    }

                    return false;
                };
            });
        }

        return Task.CompletedTask;
    }

    public override async Task OnInitialized(InitContext context)
    {
        var app = context.Application;

        // Config
        var configService = app.Services.GetRequiredService<ConfigService<CoreConfiguration>>();
        var config = configService.Get();

        // Allow MoonlightService to access the app
        var moonlightService = app.Services.GetRequiredService<MoonlightService>();
        moonlightService.Application = app;

        // Define permissions
        var permissionService = app.Services.GetRequiredService<PermissionService>();

        await permissionService.Register(999, new()
        {
            Name = "See Admin Page",
            Description = "Allows access to the admin page and the connected stats (server and user count)"
        });

        await permissionService.Register(1000, new()
        {
            Name = "Manage users",
            Description = "Allows access to users and their sessions"
        });

        await permissionService.Register(9000, new()
        {
            Name = "View exceptions",
            Description = "Allows to see the raw message of exceptions when thrown in a view"
        });

        await permissionService.Register(9998, new()
        {
            Name = "Manage admin api access",
            Description = "Allows access to manage api keys and their permissions"
        });
        
        await permissionService.Register(9999, new()
        {
            Name = "Manage system",
            Description = "Allows access to the core system if moonlight and all configuration files"
        });

        //
        app.UseStaticFiles();
        app.UseRouting();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");
        app.MapControllers();
        app.UseWebSockets();

        // Plugins
        var pluginService = app.Services.GetRequiredService<PluginService>();

        await pluginService.RegisterImplementation<IDiagnoseAction>(new DatabaseDiagnoseAction());
        await pluginService.RegisterImplementation<IDiagnoseAction>(new ConfigDiagnoseAction());
        await pluginService.RegisterImplementation<IDiagnoseAction>(new PluginsDiagnoseAction());
        await pluginService.RegisterImplementation<IDiagnoseAction>(new FeatureDiagnoseAction());
        await pluginService.RegisterImplementation<IDiagnoseAction>(new LogDiagnoseAction());

        // UI
        await pluginService.RegisterImplementation<IAdminDashboardColumn>(new UserCount());
        await pluginService.RegisterImplementation<IUserDashboardComponent>(new GreetingMessages());

        // Startup job services
        var startupJobService = app.Services.GetRequiredService<StartupJobService>();

        await startupJobService.AddJob("Default user creation", TimeSpan.FromSeconds(3), async provider =>
        {
            using var scope = provider.CreateScope();

            var configService = scope.ServiceProvider.GetRequiredService<ConfigService<CoreConfiguration>>();
            var userRepo = scope.ServiceProvider.GetRequiredService<Repository<User>>();
            var authenticationProvider = scope.ServiceProvider.GetRequiredService<IAuthenticationProvider>();

            if (!configService.Get().Authentication.UseDefaultAuthentication)
                return;

            if (userRepo.Get().Any())
                return;

            // Define credentials
            var password = Formatter.GenerateString(32);
            var username = "adminowo";
            var email = "adminowo@example.com";

            // Register user
            var registeredUser = await authenticationProvider.Register(username, email, password);

            if (registeredUser == null)
            {
                Logger.Warn("Unable to create default user. Register function returned null");
                return;
            }

            // Give user admin permissions
            var user = userRepo.Get().First(x => x.Username == username);
            user.Permissions = 9999;
            userRepo.Update(user);

            Logger.Info($"Default login: Email: '{email}' Password: '{password}'");
        });

        // Api
        if (config.Development.EnableApiReference)
            app.MapSwagger("/api/core/reference/openapi/{documentName}");

        app.UseMiddleware<ApiPermissionMiddleware>();
        
        await pluginService.RegisterImplementation<IApiDefinition>(new InternalApiDefinition());
    }

    public override Task OnUiInitialized(UiInitContext context)
    {
        context.EnablePages<CoreFeature>();

        // User pages
        context.AddSidebarItem("Dashboard", "bxs-dashboard", "/", needsExactMatch: true, index: int.MinValue);

        // Admin pages
        context.AddSidebarItem("Dashboard", "bxs-dashboard", "/admin", needsExactMatch: true, isAdmin: true,
            index: int.MinValue);
        context.AddSidebarItem("Users", "bxs-group", "/admin/users", needsExactMatch: false, isAdmin: true);
        context.AddSidebarItem("API", "bx-code-curly", "/admin/api", needsExactMatch: false, isAdmin: true);
        context.AddSidebarItem("System", "bxs-component", "/admin/sys", needsExactMatch: false, isAdmin: true);

        return Task.CompletedTask;
    }

    public override async Task OnSessionInitialized(SessionInitContext context)
    {
        var lazyLoader = context.LazyLoader;

        // - Authentication
        var cookieService = context.ServiceProvider.GetRequiredService<CookieService>();
        var identityService = context.ServiceProvider.GetRequiredService<IdentityService>();

        await lazyLoader.SetText("Authenticating");
        var token = await cookieService.GetValue("token");
        await identityService.Authenticate(token);

        // - Session
        await lazyLoader.SetText("Starting session");
        var scopedStorageService = context.ServiceProvider.GetRequiredService<ScopedStorageService>();
        var sessionService = context.ServiceProvider.GetRequiredService<SessionService>();

        var navigationManager = context.ServiceProvider.GetRequiredService<NavigationManager>();
        var alertService = context.ServiceProvider.GetRequiredService<AlertService>();

        // Build session
        var session = new Session()
        {
            AlertService = alertService,
            IdentityService = identityService,
            NavigationManager = navigationManager,
            UpdatedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Setup updating
        navigationManager.LocationChanged += (_, _) => { session.UpdatedAt = DateTime.UtcNow; };

        // Save session and session service to view storage
        scopedStorageService.Set("Session", session);
        scopedStorageService.Set("SessionService", sessionService);

        // Register session
        await sessionService.Add(session);
    }

    public override async Task OnSessionDisposed(SessionDisposeContext context)
    {
        // - Session
        // Load session from scoped storage and check if it exists, as it may not exist when the
        // session initialisation was interrupted
        var session = context.ScopedStorageService.Get<Session>("Session");

        if (session != null)
        {
            var sessionService = context.ScopedStorageService.Get<SessionService>("SessionService");

            // Unregister session
            if (sessionService != null)
                await sessionService.Remove(session);
        }
    }
}