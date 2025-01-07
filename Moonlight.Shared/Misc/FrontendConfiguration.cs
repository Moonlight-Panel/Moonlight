namespace Moonlight.Shared.Misc;

public class FrontendConfiguration
{
    public string Title { get; set; }
    public string ApiUrl { get; set; }
    public string HostEnvironment { get; set; }
    public ThemeData Theme { get; set; } = new();
    public string[] Scripts { get; set; }
    public PluginData Plugins { get; set; } = new();
    
    public class ThemeData
    {
        public Dictionary<string, string> Variables { get; set; } = new();
    }
    
    public class PluginData
    {
        public string[] Assemblies { get; set; }
        public string[] Entrypoints { get; set; }
    }
}