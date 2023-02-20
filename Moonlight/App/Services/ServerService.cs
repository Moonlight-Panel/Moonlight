using System.Security.Cryptography;
using System.Text;
using JWT.Algorithms;
using JWT.Builder;
using Logging.Net;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Files;
using Moonlight.App.Models.Files.Accesses;
using Moonlight.App.Models.Wings;
using Moonlight.App.Models.Wings.Requests;
using Moonlight.App.Models.Wings.Resources;
using Moonlight.App.Repositories;
using Moonlight.App.Repositories.Servers;

namespace Moonlight.App.Services;

public class ServerService
{
    private readonly ServerRepository ServerRepository;
    private readonly UserRepository UserRepository;
    private readonly ImageRepository ImageRepository;
    private readonly NodeRepository NodeRepository;
    private readonly WingsApiHelper WingsApiHelper;
    private readonly MessageService MessageService;
    private readonly UserService UserService;
    private readonly ConfigService ConfigService;
    private readonly WingsJwtHelper WingsJwtHelper;
    private readonly string AppUrl;

    public ServerService(
        ServerRepository serverRepository,
        WingsApiHelper wingsApiHelper,
        UserRepository userRepository,
        ImageRepository imageRepository,
        NodeRepository nodeRepository,
        MessageService messageService,
        UserService userService,
        ConfigService configService,
        WingsJwtHelper wingsJwtHelper)
    {
        ServerRepository = serverRepository;
        WingsApiHelper = wingsApiHelper;
        UserRepository = userRepository;
        ImageRepository = imageRepository;
        NodeRepository = nodeRepository;
        MessageService = messageService;
        UserService = userService;
        ConfigService = configService;
        WingsJwtHelper = wingsJwtHelper;

        AppUrl = ConfigService.GetSection("Moonlight").GetValue<string>("AppUrl");
    }

    private Server EnsureNodeData(Server s)
    {
        if (s.Node == null) // Ensure node data is available
        {
            return ServerRepository
                .Get()
                .Include(x => x.Node)
                .First(x => x.Id == s.Id);
        }
        else
            return s;
    }

    public async Task<ServerDetailsResponse> GetDetails(Server s)
    {
        Server server = EnsureNodeData(s);

        return await WingsApiHelper.Get<ServerDetailsResponse>(
            server.Node,
            $"api/servers/{server.Uuid}"
        );
    }

    public async Task SetPowerState(Server s, PowerSignal signal)
    {
        Server server = EnsureNodeData(s);

        var rawSignal = signal.ToString().ToLower();

        await WingsApiHelper.Post(server.Node, $"api/servers/{server.Uuid}/power", new ServerPowerRequest()
        {
            Action = rawSignal
        });
    }

    public async Task<ServerBackup> CreateBackup(Server server)
    {
        var serverData = ServerRepository // Ensure data
            .Get()
            .Include(x => x.Node)
            .Include(x => x.Backups)
            .First(x => x.Id == server.Id);

        var backup = new ServerBackup()
        {
            Name = $"Created at {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}",
            Uuid = Guid.NewGuid(),
            CreatedAt = DateTime.Now,
            Created = false
        };

        serverData.Backups.Add(backup);
        ServerRepository.Update(serverData);

        await WingsApiHelper.Post(serverData.Node, $"api/servers/{serverData.Uuid}/backup", new CreateBackupRequest()
        {
            Adapter = "wings",
            Uuid = backup.Uuid,
            Ignore = ""
        });

        return backup;
    }

    public Task<ServerBackup[]> GetBackups(Server server, bool forceReload = false)
    {
        if (forceReload) //TODO: Find an alternative to avoid cache and the creation of a new db context
        {
            var serverData = new ServerRepository(new DataContext(ConfigService))
                .Get()
                .Include(x => x.Backups)
                .First(x => x.Id == server.Id);

            return Task.FromResult(serverData.Backups.ToArray());
        }
        else
        {
            var serverData = ServerRepository
                .Get()
                .Include(x => x.Backups)
                .First(x => x.Id == server.Id);

            return Task.FromResult(serverData.Backups.ToArray());
        }
    }

    public async Task RestoreBackup(Server s, ServerBackup serverBackup)
    {
        Server server = EnsureNodeData(s);

        await WingsApiHelper.Post(server.Node, $"api/servers/{server.Uuid}/backup/{serverBackup.Uuid}/restore",
            new RestoreBackupRequest()
            {
                Adapter = "wings"
            });
    }

    public async Task DeleteBackup(Server server, ServerBackup serverBackup)
    {
        var serverData = ServerRepository
            .Get()
            .Include(x => x.Node)
            .Include(x => x.Backups)
            .First(x => x.Id == server.Id);

        await WingsApiHelper.Delete(serverData.Node, $"api/servers/{serverData.Uuid}/backup/{serverBackup.Uuid}",
            null);

        var backup = serverData.Backups.First(x => x.Uuid == serverBackup.Uuid);
        serverData.Backups.Remove(backup);

        ServerRepository.Update(serverData);

        await MessageService.Emit("wings.backups.delete", backup);
    }

    public Task<string> DownloadBackup(Server s, ServerBackup serverBackup)
    {
        Server server = EnsureNodeData(s);

        var token = WingsJwtHelper.Generate(server.Node.Token, claims =>
        {
            claims.Add("server_uuid", server.Uuid.ToString());
            claims.Add("backup_uuid", serverBackup.Uuid.ToString());
        });

        return Task.FromResult(
            $"https://{server.Node.Fqdn}:{server.Node.HttpPort}/download/backup?token={token}"
        );
    }

    public Task<IFileAccess> CreateFileAccess(Server s, User user) // We need the user to create the launch url
    {
        Server server = EnsureNodeData(s);

        return Task.FromResult(
            (IFileAccess)new WingsFileAccess(
                WingsApiHelper,
                server,
                user,
                WingsJwtHelper,
                AppUrl
            )
        );
    }

    public async Task<Server> Create(string name, int cpu, long memory, long disk, User u, Image i, Node? n = null, Action<Server>? modifyDetails = null)
    {
        var user = UserRepository
            .Get()
            .First(x => x.Id == u.Id);

        var image = ImageRepository
            .Get()
            .Include(x => x.Variables)
            .Include(x => x.DockerImages)
            .First(x => x.Id == i.Id);

        Node node;

        if (n == null)
        {
            node = NodeRepository.Get().Include(x => x.Allocations).First(); //TODO: Smart deploy
        }
        else
        {
            node = NodeRepository
                .Get()
                .Include(x => x.Allocations)
                .First(x => x.Id == n.Id);
        }

        NodeAllocation freeAllo;

        try
        {
            freeAllo = node.Allocations.First(a => !ServerRepository.Get()
                .SelectMany(s => s.Allocations)
                .Any(b => b.Id == a.Id)); // Thank you ChatGPT <3
        }
        catch (Exception)
        {
            throw new DisplayException("No allocation found");
        }

        if (freeAllo == null)
            throw new DisplayException("No allocation found");

        var server = new Server()
        {
            Cpu = cpu,
            Memory = memory,
            Disk = disk,
            Name = name,
            Image = image,
            Owner = user,
            Node = node,
            Uuid = Guid.NewGuid(),
            MainAllocation = freeAllo,
            Allocations = new()
            {
                freeAllo
            },
            Backups = new(),
            OverrideStartup = "",
            DockerImageIndex = image.DockerImages.FindIndex(x => x.Default)
        };

        foreach (var imageVariable in image.Variables)
        {
            server.Variables.Add(new()
            {
                Key = imageVariable.Key,
                Value = imageVariable.DefaultValue
            });
        }
        
        if(modifyDetails != null)
            modifyDetails.Invoke(server);

        var newServerData = ServerRepository.Add(server);

        try
        {
            await WingsApiHelper.Post(node, $"api/servers", new CreateServerRequest()
            {
                Uuid = newServerData.Uuid,
                StartOnCompletion = false
            });

            return newServerData;
        }
        catch (Exception e)
        {
            Logger.Error("Error creating server on wings. Deleting db model");
            Logger.Error(e);

            ServerRepository.Delete(newServerData);

            throw new Exception("Error creating server on wings");
        }
    }

    public async Task Reinstall(Server s)
    {
        Server server = EnsureNodeData(s);

        await WingsApiHelper.Post(server.Node, $"api/servers/{server.Uuid}/reinstall", null);
    }

    public async Task<Server> SftpServerLogin(int serverId, int id, string password)
    {
        var server = ServerRepository.Get().FirstOrDefault(x => x.Id == serverId);

        if (server == null) //TODO: Logging
            throw new Exception("Server not found");

        var user = await UserService.SftpLogin(id, password);

        if (server.Owner.Id == user.Id)
        {
            return server;
        }
        else
        {
            throw new Exception("User and owner id do not match");
        }
    }
}