using MoonCore.Services;
using Moonlight.ApiServer.Configuration;

namespace Moonlight.ApiServer.Helpers;

public class ApplicationStateHelper
{
    public static AppConfiguration Configuration { get; private set; }

    public static void SetConfiguration(AppConfiguration configuration) => Configuration = configuration;
}