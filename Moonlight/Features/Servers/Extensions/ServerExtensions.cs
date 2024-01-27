using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Models.Abstractions;

namespace Moonlight.Features.Servers.Extensions;

public static class ServerExtensions
{
    public static ServerConfiguration ToServerConfiguration(this Server server)
    {
        var serverConfiguration = new ServerConfiguration();

        // Set general information
        serverConfiguration.Id = server.Id;
        
        // Set variables
        serverConfiguration.Variables = server.Variables
            .ToDictionary(x => x.Key, x => x.Value);

        // Set server image
        serverConfiguration.Image = new()
        {
            OnlineDetection = server.Image.OnlineDetection,
            ParseConfigurations = server.Image.ParseConfigurations,
            StartupCommand = server.Image.StartupCommand,
            StopCommand = server.Image.StopCommand
        };

        // Find docker image by index
        ServerDockerImage dockerImage;

        if (server.DockerImageIndex >= server.Image.DockerImages.Count || server.DockerImageIndex == -1)
            dockerImage = server.Image.DockerImages.Last();
        else
            dockerImage = server.Image.DockerImages[server.DockerImageIndex];

        serverConfiguration.Image.DockerImage = dockerImage.Name;
        serverConfiguration.Image.PullDockerImage = dockerImage.AutoPull;
        
        // Set server limits
        serverConfiguration.Limits = new()
        {
            Cpu = server.Cpu,
            Memory = server.Memory,
            Disk = server.Disk
        };
        
        // Set allocations
        serverConfiguration.Allocations = server.Allocations.Select(x => new ServerConfiguration.AllocationData()
        {
            IpAddress = x.IpAddress,
            Port = x.Port
        }).ToList();

        // Set main allocation
        serverConfiguration.MainAllocation = new()
        {
            IpAddress = server.MainAllocation.IpAddress,
            Port = server.MainAllocation.Port
        };

        return serverConfiguration;
    }

    public static ServerInstallConfiguration ToServerInstallConfiguration(this Server server)
    {
        var installConfiguration = new ServerInstallConfiguration();

        installConfiguration.DockerImage = server.Image.InstallDockerImage;
        installConfiguration.Script = server.Image.InstallScript;
        installConfiguration.Shell = server.Image.InstallShell;

        return installConfiguration;
    }
}