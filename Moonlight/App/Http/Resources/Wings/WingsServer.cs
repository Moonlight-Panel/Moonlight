using System.Text.Json.Serialization;

namespace Moonlight.App.Http.Resources.Wings;

public class WingsServer
{
    [JsonPropertyName("uuid")]
    public Guid Uuid { get; set; }

    [JsonPropertyName("settings")] public WingsServerSettings Settings { get; set; } = new();

    [JsonPropertyName("process_configuration")]
    public WingsServerProcessConfiguration Process_Configuration { get; set; } = new();
    
    public class WingsServerProcessConfiguration
    {
        [JsonPropertyName("startup")] public WingsServerStartup Startup { get; set; } = new();

        [JsonPropertyName("stop")] public WingsServerStop Stop { get; set; } = new();

        [JsonPropertyName("configs")] public List<WingsServerConfig> Configs { get; set; } = new();
    }

    public class WingsServerConfig
    {
        [JsonPropertyName("parser")]
        public string Parser { get; set; }

        [JsonPropertyName("file")]
        public string File { get; set; }

        [JsonPropertyName("replace")] public List<WingsServerReplace> Replace { get; set; } = new();
    }

    public class WingsServerReplace
    {
        [JsonPropertyName("match")]
        public string Match { get; set; }

        [JsonPropertyName("replace_with")]
        public string Replace_With { get; set; }
    }

    public class WingsServerStartup
    {
        [JsonPropertyName("done")] public List<string> Done { get; set; } = new();

        [JsonPropertyName("user_interaction")] public List<object> User_Interaction { get; set; } = new();

        [JsonPropertyName("strip_ansi")]
        public bool Strip_Ansi { get; set; }
    }

    public class WingsServerStop
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class WingsServerSettings
    {
        [JsonPropertyName("uuid")]
        public Guid Uuid { get; set; }

        [JsonPropertyName("suspended")]
        public bool Suspended { get; set; }

        [JsonPropertyName("environment")] public Dictionary<string, string> Environment { get; set; } = new();

        [JsonPropertyName("invocation")]
        public string Invocation { get; set; }

        [JsonPropertyName("skip_egg_scripts")]
        public bool Skip_Egg_Scripts { get; set; }

        [JsonPropertyName("build")] public WingsServerBuild Build { get; set; } = new();

        [JsonPropertyName("container")] public WingsServerContainer Container { get; set; } = new();

        [JsonPropertyName("allocations")] public WingsServerAllocations Allocations { get; set; } = new();

        [JsonPropertyName("mounts")] public List<object> Mounts { get; set; } = new();

        [JsonPropertyName("egg")] public WingsServerEgg Egg { get; set; } = new();
    }

    public class WingsServerAllocations
    {
        [JsonPropertyName("default")] public WingsServerDefault Default { get; set; } = new();

        [JsonPropertyName("mappings")] public WingsServerMappings Mappings { get; set; } = new();
    }

    public class WingsServerDefault
    {
        [JsonPropertyName("ip")]
        public string Ip { get; set; }

        [JsonPropertyName("port")]
        public long Port { get; set; }
    }

    public class WingsServerMappings
    {
        [JsonPropertyName("0.0.0.0")] public List<long> Ports { get; set; } = new();
    }

    public class WingsServerBuild
    {
        [JsonPropertyName("memory_limit")]
        public long Memory_Limit { get; set; }

        [JsonPropertyName("swap")]
        public long Swap { get; set; }

        [JsonPropertyName("io_weight")]
        public long Io_Weight { get; set; }

        [JsonPropertyName("cpu_limit")]
        public long Cpu_Limit { get; set; }

        [JsonPropertyName("threads")]
        public object Threads { get; set; }

        [JsonPropertyName("disk_space")]
        public long Disk_Space { get; set; }

        [JsonPropertyName("oom_disabled")]
        public bool Oom_Disabled { get; set; }
        
        [JsonPropertyName("oom_killer")]
        public bool Oom_Killer { get; set; }
    }

    public class WingsServerContainer
    {
        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("oom_disabled")]
        public bool Oom_Disabled { get; set; }

        [JsonPropertyName("requires_rebuild")]
        public bool Requires_Rebuild { get; set; }
    }

    public class WingsServerEgg
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("file_denylist")] public List<object> File_Denylist { get; set; } = new();
    }
}