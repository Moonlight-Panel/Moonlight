using MoonCore.Services;
using Moonlight.Shared.Models;

namespace Moonlight.ApiServer.App.Helpers;

public static class ExecutionMetadata
{
    public static bool IsRunningEf { get; set; } = false;
    public static ConfigService<AppConfiguration> ConfigService { get; set; }
}