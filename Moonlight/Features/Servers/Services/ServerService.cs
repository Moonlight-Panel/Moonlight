using Microsoft.EntityFrameworkCore;
using MoonCore.Abstractions;
using MoonCore.Attributes;
using MoonCore.Exceptions;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.Core.Configuration;
using Moonlight.Core.Database.Entities;
using Moonlight.Core.Services;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;
using Moonlight.Features.Servers.Api.Packets;
using Moonlight.Features.Servers.Api.Resources;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Extensions;
using Moonlight.Features.Servers.Helpers;
using Moonlight.Features.Servers.Models.Enums;

namespace Moonlight.Features.Servers.Services;

[Singleton]
public class ServerService
{
    public ServerConsoleService Console => ServiceProvider.GetRequiredService<ServerConsoleService>();
    public ServerBackupService Backup => ServiceProvider.GetRequiredService<ServerBackupService>();
    public ServerScheduleService Schedule => ServiceProvider.GetRequiredService<ServerScheduleService>();
    
    public NodeService NodeService => ServiceProvider.GetRequiredService<NodeService>();

    private readonly IServiceProvider ServiceProvider;
    private readonly ILogger<ServerService> Logger;

    public ServerService(IServiceProvider serviceProvider, ILogger<ServerService> logger)
    {
        ServiceProvider = serviceProvider;
        Logger = logger;
    }

    public async Task Sync(Server server)
    {
        using var httpClient = server.CreateHttpClient(ServiceProvider);
        await httpClient.Post($"servers/{server.Id}/sync");
    }

    public async Task<ServerState> GetState(Server server)
    {
        using var httpClient = server.CreateHttpClient(ServiceProvider);
        var state = await httpClient.GetAsString($"servers/{server.Id}/state");

        return Enum.Parse<ServerState>(state, true);
    }

    public async Task SyncDelete(Server server)
    {
        using var httpClient = server.CreateHttpClient(ServiceProvider);
        await httpClient.DeleteAsString($"servers/{server.Id}");
    }

    // Note:
    // This server model is just used as a dto and is not saved in the database anywhere
    public async Task<Server> Create(Server form)
    {
        using var scope = ServiceProvider.CreateScope();

        // Load all dependencies from the di
        var serverRepo = scope.ServiceProvider.GetRequiredService<Repository<Server>>();
        var imageRepo = scope.ServiceProvider.GetRequiredService<Repository<ServerImage>>();
        var nodeRepo = scope.ServiceProvider.GetRequiredService<Repository<ServerNode>>();
        var userRepo = scope.ServiceProvider.GetRequiredService<Repository<User>>();
        var allocationRepo = scope.ServiceProvider.GetRequiredService<Repository<ServerAllocation>>();
        var serverService = scope.ServiceProvider.GetRequiredService<ServerService>();

        // Load and validate image
        var image = imageRepo
            .Get()
            .Include(x => x.DockerImages)
            .Include(x => x.Variables)
            .First(x => x.Id == form.Image.Id);

        // Load node
        var node = nodeRepo.Get().First(x => x.Id == form.Node.Id);

        // Check if node is available
        try
        {
            await NodeService.GetStatus(node);
        }
        catch (Exception e)
        {
            Logger.LogWarning("Could not establish to the node with the id {nodeId}: {e}", node.Id, e);
            
            throw new DisplayException($"Could not establish connection to the node: {e.Message}");
        }
        
        // Load user
        var user = userRepo.Get().First(x => x.Id == form.Owner.Id);

        // Load and validate server allocations
        List<ServerAllocation> allocations = new();

        var amountOfAutoAllocations = image.AllocationsNeeded;

        if (form.Allocations.Count > 0)
        {
            amountOfAutoAllocations -= form.Allocations.Count;

            foreach (var serverAllocation in form.Allocations) // Resolve all allocations specified in the form in the current scope
            {
                var allocationInCurrentScope = allocationRepo.Get().First(x => x.Id == serverAllocation.Id);
                allocations.Add(allocationInCurrentScope);
            }
        }

        if (amountOfAutoAllocations > 0) // Resolve all other allocations which are required automatically
        {
            if (false)
            {
                throw new DisplayException(
                    "The dedicated ip mode has not been implemented yet. Please disable the dedicated ip option in the product configuration");
            }
            else
            {
                var autoAllocations = allocationRepo
                    .Get()
                    .FromSqlRaw(
                        $"SELECT * FROM `ServerAllocations` WHERE ServerId IS NULL AND ServerNodeId={node.Id} LIMIT {image.AllocationsNeeded + form.Allocations.Count}")
                    .ToArray();

                var addedAutoAllocations = 0;
                
                foreach (var autoAllocation in autoAllocations)
                {
                    if(addedAutoAllocations >= amountOfAutoAllocations)
                        break;
                    
                    if(form.Allocations.Any(x => x.Id == autoAllocation.Id))
                        continue;
                    
                    allocations.Add(autoAllocation);
                    addedAutoAllocations++;
                }
            }
        }

        if (allocations.Count < 1 || allocations.Count < image.AllocationsNeeded)
            throw new DisplayException($"Not enough free allocations found on node '{node.Name}'");

        // Build server db model

        var server = new Server()
        {
            Cpu = form.Cpu,
            Memory = form.Memory,
            Disk = form.Disk,
            Node = node,
            MainAllocation = allocations.First(),
            Image = image,
            OverrideStartupCommand = null,
            DockerImageIndex = image.DefaultDockerImage,
            Owner = user,
            Name = form.Name,
            UseVirtualDisk = form.UseVirtualDisk
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

        var finalModel = serverRepo.Add(server);

        await serverService.Sync(server);
        await serverService.Console.SendAction(server, PowerAction.Install, runAsync: true);

        return finalModel;
    }

    public async Task Delete(Server s, bool safeDelete = true)
    {
        // Delete backups
        await DeleteBackups(s);

        // Delete server
        using var scope = ServiceProvider.CreateScope();

        var serverRepo = scope.ServiceProvider.GetRequiredService<Repository<Server>>();
        var variableRepo = scope.ServiceProvider.GetRequiredService<Repository<ServerVariable>>();
        var serverService = scope.ServiceProvider.GetRequiredService<ServerService>();

        // Delete server
        var serverWithData = serverRepo
            .Get()
            .Include(x => x.Variables)
            .Include(x => x.Allocations)
            .First(x => x.Id == s.Id);

        try
        {
            // Note:
            // We send the request to the node first in order to ensure its
            // actually deleted before we continue to delete it in the database

            await serverService.SyncDelete(serverWithData);
        }
        catch (Exception)
        {
            if (safeDelete)
                throw;
        }

        // Database delete
        var variables = serverWithData.Variables.ToArray();

        serverWithData.Variables.Clear();
        serverWithData.Allocations.Clear();

        serverRepo.Update(serverWithData);

        foreach (var variable in variables)
        {
            try
            {
                variableRepo.Delete(variable);
            }
            catch (Exception)
            {
                /* ignore errors here as they should not fail the operation */
            }
        }

        serverRepo.Delete(serverWithData);
    }

    private async Task DeleteBackups(Server s)
    {
        using var scope = ServiceProvider.CreateScope();

        var serverRepo = scope.ServiceProvider.GetRequiredService<Repository<Server>>();

        // Delete backups
        var serverWithBackups = serverRepo
            .Get()
            .Include(x => x.Backups)
            .First(x => x.Id == s.Id);

        foreach (var backup in serverWithBackups.Backups)
            await Backup.Delete(serverWithBackups, backup, false);
    }

    public Task<BaseFileAccess> OpenFileAccess(Server s)
    {
        using var scope = ServiceProvider.CreateScope();

        var serverRepo = scope.ServiceProvider.GetRequiredService<Repository<Server>>();

        var server = serverRepo
            .Get()
            .Include(x => x.Node)
            .First(x => x.Id == s.Id);

        var protocol = server.Node.Ssl ? "https" : "http";
        var remoteUrl = $"{protocol}://{server.Node.Fqdn}:{server.Node.HttpPort}/";
        
        var result = new BaseFileAccess(
            new ServerApiFileActions(remoteUrl, server.Node.Token, server.Id)
        );

        return Task.FromResult(result);
    }

    public async Task<ServerListItem[]> GetServersList(ServerNode node, bool includeOffline = false)
    {
        using var httpClient = node.CreateHttpClient();
        return await httpClient.Get<ServerListItem[]>($"servers/list?includeOffline={includeOffline}");
    }

    public async Task<ServerStats> GetStats(Server server)
    {
        using var httpClient = server.CreateHttpClient(ServiceProvider);
        return await httpClient.Get<ServerStats>($"servers/{server.Id}/stats");
    }
}