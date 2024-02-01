using Microsoft.EntityFrameworkCore;
using MoonCore.Abstractions;
using MoonCore.Exceptions;
using MoonCore.Helpers;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Models.Enums;
using Moonlight.Features.Servers.Services;
using Moonlight.Features.ServiceManagement.Entities;
using Moonlight.Features.ServiceManagement.Models.Abstractions;
using Newtonsoft.Json;

namespace Moonlight.Features.Servers.Actions;

public class ServerActions : ServiceActions
{
    public override async Task Create(IServiceProvider provider, Service service)
    {
        // Load all dependencies from the di
        var serverRepo = provider.GetRequiredService<Repository<Server>>();
        var imageRepo = provider.GetRequiredService<Repository<ServerImage>>();
        var nodeRepo = provider.GetRequiredService<Repository<ServerNode>>();
        var allocationRepo = provider.GetRequiredService<Repository<ServerAllocation>>();
        var serverService = provider.GetRequiredService<ServerService>();
        
        // Parse the configuration file
        var config =
            JsonConvert.DeserializeObject<ServerConfig>(service.ConfigJsonOverride ?? service.Product.ConfigJson)!;
        
        // Load and validate image
        
        var image = imageRepo
            .Get()
            .Include(x => x.DockerImages)
            .Include(x => x.Variables)
            .FirstOrDefault(x => x.Id == config.ImageId);

        if (image == null)
            throw new DisplayException("An image with this is is not found");
        
        // Load and validate node

        ServerNode? node = null;

        if (config.NodeId != 0)
        {
            node = nodeRepo
                .Get()
                .FirstOrDefault(x => x.Id == config.NodeId);
        }

        if (node == null)
        {
            //TODO: Implement auto deploy
            throw new DisplayException("Auto deploy has not been implemented yet. Please specify the node id in the product configuration");
        }
        
        // Load and validate server allocations
        ServerAllocation[] allocations = Array.Empty<ServerAllocation>();

        if (config.DedicatedIp)
        {
            throw new DisplayException("The dedicated ip mode has not been implemented yet. Please disable the dedicated ip option in the product configuration");
        }
        else
        {
            allocations = allocationRepo
                .Get()
                .FromSqlRaw(
                    $"SELECT * FROM `ServerAllocations` WHERE ServerId IS NULL AND ServerNodeId={node.Id} LIMIT {image.AllocationsNeeded}")
                .ToArray();
        }

        if (allocations.Length < 1 || allocations.Length < image.AllocationsNeeded)
            throw new DisplayException($"Not enough free allocations found on node '{node.Name}'");

        // Build server db model

        var server = new Server()
        {
            Service = service,
            Cpu = config.Cpu,
            Memory = config.Memory,
            Disk = config.Disk,
            Node = node,
            MainAllocation = allocations.First(),
            Image = image,
            OverrideStartupCommand = null,
            DockerImageIndex = image.DefaultDockerImageIndex
        };

        // Add allocations
        foreach (var allocation in allocations)
            server.Allocations.Add(allocation);
        
        // Add variables
        foreach (var variable in image.Variables)
        {
            server.Variables.Add(new()
            {
                Key = variable.Key,
                Value = variable.DefaultValue
            });
        }

        serverRepo.Add(server);

        await serverService.Sync(server);
        await serverService.SendPowerAction(server, PowerAction.Install);
    }

    public override Task Update(IServiceProvider provider, Service service)
    {
        throw new NotImplementedException();
    }

    public override async Task Delete(IServiceProvider provider, Service service)
    {
        // Load dependencies from di
        var serverRepo = provider.GetRequiredService<Repository<Server>>();
        var serverService = provider.GetRequiredService<ServerService>();
        var serverVariableRepo = provider.GetRequiredService<Repository<ServerVariable>>();

        // Load server
        var server = serverRepo
            .Get()
            .Include(x => x.Variables)
            .Include(x => x.MainAllocation)
            .FirstOrDefault(x => x.Service.Id == service.Id);
        
        // Check if server already has been deleted
        if (server == null)
        {
            Logger.Warn($"Server for service {service.Id} is missing when trying to delete the service. Maybe it already has been deleted");
            return;
        }

        // Notify the node
        await serverService.SyncDelete(server);
        
        // Clear and delete the variables
        var variables = server.Variables.ToArray();
        
        server.Variables.Clear();
        
        serverRepo.Update(server);

        try
        {
            foreach (var variable in variables)
                serverVariableRepo.Delete(variable);
        }
        catch (Exception) { /* ignored, as we dont want a operation to fail which just deletes some old data */ }
        
        // Delete the model
        serverRepo.Delete(server);
    }
}