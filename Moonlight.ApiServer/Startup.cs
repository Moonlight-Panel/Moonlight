using System.Text.Json;
using MoonCore.Authentication;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Extensions;
using MoonCore.Extended.Helpers;
using MoonCore.Extensions;
using MoonCore.Helpers;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Helpers;
using Moonlight.ApiServer.Interfaces.Startup;

namespace Moonlight.ApiServer;

public static class Startup
{
    #region Logging

    public static async Task ConfigureLogging(IHostApplicationBuilder builder)
    {
        // Create logging path
        Directory.CreateDirectory(PathBuilder.Dir("storage", "logs"));
        
        // Configure application logging
        builder.Logging.ClearProviders();

        builder.Logging.AddMoonCore(configuration =>
        {
            configuration.Console.Enable = true;
            configuration.Console.EnableAnsiMode = true;

            configuration.FileLogging.Enable = true;
            configuration.FileLogging.Path = PathBuilder.File("storage", "logs", "moonlight.log");
            configuration.FileLogging.EnableLogRotation = true;
            configuration.FileLogging.RotateLogNameTemplate = PathBuilder.File("storage", "logs", "moonlight.log.{0}");
        });

        // Logging levels
        var logConfigPath = PathBuilder.File("storage", "logConfig.json");

        // Ensure logging config, add a default one is missing
        if (!File.Exists(logConfigPath))
        {
            var logLevels = new Dictionary<string, string>
            {
                { "Default", "Information" },
                { "Microsoft.AspNetCore", "Warning" },
                { "System.Net.Http.HttpClient", "Warning" }
            };

            var logLevelsJson = JsonSerializer.Serialize(logLevels);
            var logConfig = "{\"LogLevel\":" + logLevelsJson + "}";
            await File.WriteAllTextAsync(logConfigPath, logConfig);
        }

        builder.Logging.AddConfiguration(await File.ReadAllTextAsync(logConfigPath));
    }

    #endregion

    #region Token Authentication

    public static Task ConfigureTokenAuthentication(IHostApplicationBuilder builder, AppConfiguration config)
    {
        builder.Services.AddTokenAuthentication(configuration =>
        {
            configuration.AccessSecret = config.Authentication.AccessSecret;
            configuration.DataLoader = async (data, provider, context) =>
            {
                if (!data.TryGetValue("userId", out var userIdStr) || !userIdStr.TryGetInt32(out var userId))
                    return false;

                var userRepo = provider.GetRequiredService<DatabaseRepository<User>>();
                var user = userRepo.Get().FirstOrDefault(x => x.Id == userId);

                if (user == null)
                    return false;

                // Load permissions, handle empty values
                var permissions = JsonSerializer.Deserialize<string[]>(
                    string.IsNullOrEmpty(user.PermissionsJson) ? "[]" : user.PermissionsJson
                ) ?? [];

                // Save permission state
                context.User = new PermClaimsPrinciple(permissions)
                {
                    IdentityModel = user
                };

                return true;
            };
        });

        return Task.CompletedTask;
    }

    public static Task UseTokenAuthentication(IApplicationBuilder builder)
    {
        builder.UseTokenAuthentication();
        return Task.CompletedTask;
    }

    #endregion

    #region Database

    public static async Task ConfigureDatabase(IHostApplicationBuilder builder, ILoggerFactory loggerFactory,
        IDatabaseStartup[] databaseStartups)
    {
        var logger = loggerFactory.CreateLogger<DatabaseHelper>();
        var databaseHelper = new DatabaseHelper(logger);

        var databaseCollection = new DatabaseContextCollection();

        foreach (var databaseStartup in databaseStartups)
            await databaseStartup.ConfigureDatabase(databaseCollection);

        foreach (var database in databaseCollection)
        {
            databaseHelper.AddDbContext(database);
            builder.Services.AddScoped(database);
        }

        databaseHelper.GenerateMappings();

        builder.Services.AddSingleton(databaseHelper);
        builder.Services.AddScoped(typeof(DatabaseRepository<>));
        builder.Services.AddScoped(typeof(CrudHelper<,>));
    }

    public static async Task PrepareDatabase(IApplicationBuilder builder)
    {
        using var scope = builder.ApplicationServices.CreateScope();
        var databaseHelper = scope.ServiceProvider.GetRequiredService<DatabaseHelper>();

        await databaseHelper.EnsureMigrated(scope.ServiceProvider);
    }

    #endregion

    #region OAuth2

    public static Task ConfigureOAuth2(IHostApplicationBuilder builder, ILogger logger, AppConfiguration config)
    {
        builder.Services.AddOAuth2Consumer(configuration =>
        {
            configuration.ClientId = config.Authentication.OAuth2.ClientId;
            configuration.ClientSecret = config.Authentication.OAuth2.ClientSecret;
            configuration.AuthorizationRedirect =
                config.Authentication.OAuth2.AuthorizationRedirect ?? $"{config.PublicUrl}/auth";

            configuration.AccessEndpoint =
                config.Authentication.OAuth2.AccessEndpoint ?? $"{config.PublicUrl}/oauth2/access";
            configuration.RefreshEndpoint =
                config.Authentication.OAuth2.RefreshEndpoint ?? $"{config.PublicUrl}/oauth2/refresh";

            if (config.Authentication.UseLocalOAuth2)
            {
                configuration.AuthorizationEndpoint = config.Authentication.OAuth2.AuthorizationRedirect ??
                                                      $"{config.PublicUrl}/oauth2/authorize";
            }
            else
            {
                if (config.Authentication.OAuth2.AuthorizationUri == null)
                    logger.LogWarning(
                        "The 'AuthorizationUri' for the oauth2 client is not set. If you want to use an external oauth2 provider, you need to specify this url. If you want to use the local oauth2 service, set 'UseLocalOAuth2Service' to true");

                configuration.AuthorizationEndpoint = config.Authentication.OAuth2.AuthorizationUri!;
            }
        });

        if (!config.Authentication.UseLocalOAuth2) return Task.CompletedTask;

        logger.LogInformation("Using local oauth2 provider");

        builder.Services.AddOAuth2Provider(configuration =>
        {
            configuration.AccessSecret = config.Authentication.LocalOAuth2.AccessSecret;
            configuration.RefreshSecret = config.Authentication.LocalOAuth2.RefreshSecret;

            configuration.ClientId = config.Authentication.OAuth2.ClientId;
            configuration.ClientSecret = config.Authentication.OAuth2.ClientSecret;
            configuration.CodeSecret = config.Authentication.LocalOAuth2.CodeSecret;
            configuration.AuthorizationRedirect =
                config.Authentication.OAuth2.AuthorizationRedirect ?? $"{config.PublicUrl}/auth";
            configuration.AccessTokenDuration = 60;
            configuration.RefreshTokenDuration = 3600;
        });

        return Task.CompletedTask;
    }

    #endregion
}