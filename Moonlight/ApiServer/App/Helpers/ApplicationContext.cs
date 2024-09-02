using MoonCore.Services;
using Moonlight.Shared.Models;

namespace Moonlight.ApiServer.App.Helpers;

public static class ApplicationContext
{
    public static bool IsRunningEfMigration { get; set; } = false;
    public static ConfigService<AppConfiguration> ConfigService { get; set; }
}