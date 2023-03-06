using System.Text;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Http.Resources.Wings;
using Moonlight.App.Repositories;
using Moonlight.App.Repositories.Servers;

namespace Moonlight.App.Helpers;

public class WingsServerConverter
{
    private readonly ServerRepository ServerRepository;
    private readonly ImageRepository ImageRepository;

    public WingsServerConverter(ServerRepository serverRepository, ImageRepository imageRepository)
    {
        ServerRepository = serverRepository;
        ImageRepository = imageRepository;
    }

    public WingsServer FromServer(Server s)
    {
        var server = ServerRepository
            .Get()
            .Include(x => x.Allocations)
            .Include(x => x.Backups)
            .Include(x => x.Variables)
            .Include(x => x.Image)
            .Include(x => x.MainAllocation)
            .First(x => x.Id == s.Id);
        
        var wingsServer = new WingsServer
        {
            Uuid = server.Uuid
        };

        // Allocations
        var def = server.MainAllocation;

        wingsServer.Settings.Allocations.Default.Ip = "0.0.0.0";
        wingsServer.Settings.Allocations.Default.Port = def.Port;

        foreach (var a in server.Allocations)
        {
            wingsServer.Settings.Allocations.Mappings.Ports.Add(a.Port);
        }
        
        // Build
        wingsServer.Settings.Build.Swap = server.Memory * 2;
        wingsServer.Settings.Build.Threads = null!;
        wingsServer.Settings.Build.Cpu_Limit = server.Cpu;
        wingsServer.Settings.Build.Disk_Space = server.Disk;
        wingsServer.Settings.Build.Io_Weight = 500;
        wingsServer.Settings.Build.Memory_Limit = server.Memory;
        wingsServer.Settings.Build.Oom_Disabled = true;

        var image = ImageRepository
            .Get()
            .Include(x => x.DockerImages)
            .First(x => x.Id == server.Image.Id);
        
        // Container
        wingsServer.Settings.Container.Image = image.DockerImages[server.DockerImageIndex].Name;

        // Egg
        wingsServer.Settings.Egg.Id = image.Uuid;

        // Settings
        wingsServer.Settings.Skip_Egg_Scripts = false;
        wingsServer.Settings.Suspended = server.Suspended;
        wingsServer.Settings.Invocation = string.IsNullOrEmpty(server.OverrideStartup) ? image.Startup : server.OverrideStartup;
        wingsServer.Settings.Uuid = server.Uuid;


        // Environment
        foreach (var v in server.Variables)
        {
            if (!wingsServer.Settings.Environment.ContainsKey(v.Key))
            {
                wingsServer.Settings.Environment.Add(v.Key, v.Value);
            }
        }
        
        // Stop
        if (image.StopCommand.StartsWith("!"))
        {
            wingsServer.Process_Configuration.Stop.Type = "stop";
            wingsServer.Process_Configuration.Stop.Value = null!;
        }
        else
        {
            wingsServer.Process_Configuration.Stop.Type = "command";
            wingsServer.Process_Configuration.Stop.Value = image.StopCommand;
        }
        
        // Done
        
        wingsServer.Process_Configuration.Startup.Done = new() { image.StartupDetection };
        wingsServer.Process_Configuration.Startup.Strip_Ansi = false;
        wingsServer.Process_Configuration.Startup.User_Interaction = new();
        
        // Configs
        var configData = new ConfigurationBuilder().AddJsonStream(
            new MemoryStream(Encoding.ASCII.GetBytes(image.ConfigFiles!))
        ).Build();

        foreach (var child in configData.GetChildren())
        {
            List<WingsServer.WingsServerReplace> replaces = new();

            foreach (var section in child.GetSection("find").GetChildren())
            {
                replaces.Add(new()
                {
                    Match = section.Key,
                    Replace_With = section.Value
                        .Replace("{{server.build.default.port}}", def.Port.ToString())
                });
            }
            
            wingsServer.Process_Configuration.Configs.Add(new()
            {
                Parser = child.GetValue<string>("parser"),
                File = child.Key,
                Replace = replaces
            });
        }

        return wingsServer;
    }
}