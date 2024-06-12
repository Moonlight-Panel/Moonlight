using Microsoft.EntityFrameworkCore;
using MoonCore.Abstractions;
using MoonCore.Helpers;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Exceptions;
using Moonlight.Features.Servers.Models.Abstractions;

namespace Moonlight.Features.Servers.Extensions;

public static class ServerExtensions
{
    public static ServerConfiguration ToServerConfiguration(this Server server)
    {
        // Enforce id based docker image index
        server.Image.DockerImages = server.Image.DockerImages
            .OrderBy(x => x.Id)
            .ToList();
        
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
            ParseConfigurations = server.Image.ParseConfiguration,
            StartupCommand = server.Image.StartupCommand,
            StopCommand = server.Image.StopCommand
        };

        // Find docker image by index
        ServerDockerImage dockerImage;

        if (server.DockerImageIndex >= server.Image.DockerImages.Count || server.DockerImageIndex == -1)
        {
            dockerImage = server.Image.DockerImages
                .Last();
        }
        else
        {
            dockerImage = server.Image.DockerImages
                .ElementAt(server.DockerImageIndex);
        }

        serverConfiguration.Image.DockerImage = dockerImage.Name;
        serverConfiguration.Image.PullDockerImage = dockerImage.AutoPull;
        
        // Set server limits
        serverConfiguration.Limits = new()
        {
            Cpu = server.Cpu,
            Memory = server.Memory,
            Disk = server.Disk,
            UseVirtualDisk = server.UseVirtualDisk
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
        
        // Private networks
        if (server.Network == null)
        {
            serverConfiguration.Network = new()
            {
                Enable = false
            };
        }
        else
        {
            serverConfiguration.Network = new()
            {
                Enable = true,
                Id = server.Network.Id
            };
        }

        // Public network
        serverConfiguration.Network.DisablePublic = server.DisablePublicNetwork;

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

    public static HttpApiClient<NodeException> CreateHttpClient(this Server server, IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var serverRepo = scope.ServiceProvider.GetRequiredService<Repository<Server>>();

        var serverWithNode = serverRepo
            .Get()
            .Include(x => x.Node)
            .First(x => x.Id == server.Id);

        return serverWithNode.Node.CreateHttpClient();
    }
}