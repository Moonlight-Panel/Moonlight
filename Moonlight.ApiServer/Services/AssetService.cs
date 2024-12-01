using MoonCore.Attributes;

namespace Moonlight.ApiServer.Services;

[Singleton]
public class AssetService
{
    public readonly List<string> CssFiles = new();
    public readonly List<string> JavascriptFiles = new();
}