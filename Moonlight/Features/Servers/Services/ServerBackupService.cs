using Microsoft.EntityFrameworkCore;
using MoonCore.Abstractions;
using MoonCore.Attributes;
using MoonCore.Services;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Extensions;
using Moonlight.Features.Servers.Models.Enums;

namespace Moonlight.Features.Servers.Services;

[Singleton]
public class ServerBackupService
{
    private readonly IServiceProvider ServiceProvider;

    public ServerBackupService(
        IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public async Task<ServerBackup> Create(Server s)
    {
        using var scope = ServiceProvider.CreateScope();

        var serverRepo = scope.ServiceProvider.GetRequiredService<Repository<Server>>();
        var server = serverRepo.Get().First(x => x.Id == s.Id);

        var timestamp = DateTime.UtcNow;
        
        server.Backups.Add(new ()
        {
            Successful = false,
            Size = 0,
            CreatedAt = timestamp
        });

        serverRepo.Update(server);

        var finalBackup = serverRepo
            .Get()
            .Include(x => x.Backups)
            .First(x => x.Id == s.Id)
            .Backups
            .First(x => x.CreatedAt == timestamp);

        try
        {
            using var httpClient = server.CreateHttpClient(ServiceProvider);
            await httpClient.Post($"servers/{server.Id}/backups/{finalBackup.Id}");
        }
        catch (Exception)
        {
            // Delete if an error occurs
            
            var backupRepo = scope.ServiceProvider.GetRequiredService<Repository<ServerBackup>>();
            backupRepo.Delete(finalBackup);
            
            throw;
        }

        return finalBackup;
    }

    public async Task Delete(Server s, ServerBackup b, bool safeDelete = true)
    {
        using var scope = ServiceProvider.CreateScope();

        var serverRepo = scope.ServiceProvider.GetRequiredService<Repository<Server>>();
        var backupRepo = scope.ServiceProvider.GetRequiredService<Repository<ServerBackup>>();
        
        var server = serverRepo
            .Get()
            .Include(x => x.Backups)
            .First(x => x.Id == s.Id);
        
        var backup = server
            .Backups
            .First(x => x.Id == b.Id);

        try
        {
            using var httpClient = server.CreateHttpClient(ServiceProvider);
            await httpClient.DeleteAsString($"servers/{server.Id}/backups/{backup.Id}");
        }
        catch (Exception)
        {
            if (safeDelete)
                throw;
        }

        server.Backups.Remove(backup);
        serverRepo.Update(server);

        try
        {
            backupRepo.Delete(backup);
        }
        catch (Exception) { /* this should not fail the operation */ }
    }

    public async Task<string> GetDownloadUrl(Server server, ServerBackup backup)
    {
        // Get remote url
        using var scope = ServiceProvider.CreateScope();
        var serverRepo = scope.ServiceProvider.GetRequiredService<Repository<Server>>();
        
        var serverWithNode = serverRepo
            .Get()
            .Include(x => x.Node)
            .First(x => x.Id == server.Id);

        var node = serverWithNode.Node;
        
        var protocol = node.Ssl ? "https" : "http";
        var remoteUrl = $"{protocol}://{node.Fqdn}:{node.HttpPort}/";
        
        // Build jwt
        var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
        var jwtService = node.CreateJwtService(loggerFactory);

        var jwt = await jwtService.Create(data =>
        {
            data.Add("BackupId", backup.Id.ToString());
        }, ServersJwtType.BackupDownload, TimeSpan.FromMinutes(5));
        
        // Build url
        return $"{remoteUrl}servers/{server.Id}/backups/{backup.Id}?downloadToken={jwt}";
    }
    
    public async Task Restore(Server server, ServerBackup backup)
    {
        using var httpClient = server.CreateHttpClient(ServiceProvider);
        await httpClient.Patch($"servers/{server.Id}/backups/{backup.Id}");
    }
}