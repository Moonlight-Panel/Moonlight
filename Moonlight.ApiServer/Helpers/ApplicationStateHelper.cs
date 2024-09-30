using MoonCore.Services;
using Moonlight.ApiServer.Configuration;

namespace Moonlight.ApiServer.Helpers;

public class ApplicationStateHelper
{
    public static ConfigService<AppConfiguration>? Configuration { get; private set; }

    public static void SetConfiguration(ConfigService<AppConfiguration>? configuration) => Configuration = configuration;
}