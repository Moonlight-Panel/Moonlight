using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MineStatLib;
using Moonlight.App.Database.Entities;
using Moonlight.App.Models.Daemon.Resources;
using Moonlight.App.Models.Wings;
using Moonlight.App.Repositories;
using Moonlight.App.Repositories.Servers;
using Logging.Net;
using Newtonsoft.Json;

namespace Moonlight.App.Services;

public class CleanupService
{
    public DateTime StartedAt { get; private set; }
    public DateTime CompletedAt { get; private set; }
    public int ServersCleaned { get; private set; }
    public int CleanupsPerformed { get; private set; }
    public int ServersRunning { get; private set; }
    public bool IsRunning { get; private set; }
    public bool Activated { get; set; }
    public int PercentProgress { get; private set; } = 100;
    public string Status { get; private set; } = "N/A";

    private Task PerformTask;
    private readonly ConfigService ConfigService;
    private readonly IServiceScopeFactory ServiceScopeFactory;

    private int RequiredCpu;
    private long RequiredMemory;
    private int WaitTime;

    public EventHandler OnUpdated;
    
    public CleanupService(ConfigService configService, IServiceScopeFactory serviceScopeFactory)
    {
        ServiceScopeFactory = serviceScopeFactory;
        ConfigService = configService;

        var config = configService.GetSection("Moonlight").GetSection("Cleanup");

        RequiredCpu = config.GetValue<int>("Cpu");
        RequiredMemory = config.GetValue<long>("Memory");
        WaitTime = config.GetValue<int>("Wait");

        if (!ConfigService.DebugMode)
            Task.Run(Start);
    }
    
    private void Start()
    {
        StartedAt = DateTime.Now;
        CompletedAt = DateTime.Now;
        IsRunning = false;
        Activated = true;

        DoWaiting();
    }
    
    private async void DoWaiting()
    {
        while (true)
        {
            if (Activated)
                await Perform();

            try
            {
                await Task.Delay((int) TimeSpan.FromMinutes(WaitTime).TotalMilliseconds);
            }
            catch (Exception ex)
            {
            }
        }
    }
    
    public async Task TriggerPerform()
    {
        if (IsRunning)
            return;

        PerformTask = new Task(async () => await Perform());
        PerformTask.Start();
    }

    private async Task Perform()
    {
        if (IsRunning)
            return;

        IsRunning = true;
        StartedAt = DateTime.Now;
        ServersRunning = 0;

        OnUpdated?.Invoke(this, null);

        using (var scope = ServiceScopeFactory.CreateScope())
        {
            // Setup time measure
            var watch = new Stopwatch();
            watch.Start();
            
            // Get repos from dependency injection
            var serverRepository = scope.ServiceProvider.GetRequiredService<ServerRepository>();
            var nodeRepository = scope.ServiceProvider.GetRequiredService<NodeRepository>();
            var cleanupExceptionRepository = scope.ServiceProvider.GetRequiredService<CleanupExceptionRepository>();
            var nodeService = scope.ServiceProvider.GetRequiredService<NodeService>();
            var imageRepo = scope.ServiceProvider.GetRequiredService<ImageRepository>();
            var serverService = scope.ServiceProvider.GetRequiredService<ServerService>();

            // Fetching data from mysql
            var servers = serverRepository.Get()
                .Include(x => x.Image)
                .ToArray();
            var nodes = nodeRepository.Get().ToArray();
            var exceptions = cleanupExceptionRepository.Get().ToArray();
            var images = imageRepo.Get().ToArray();

            var nodeCount = nodes.Count();

            // We use this counter for the foreach loops
            int counter = 0;
            PercentProgress = 0;

            // Fetching data from nodes so we know what nodes to scan
            var nodeContainers = new Dictionary<Node, ContainerStats.Container[]>();
            
            Status = "Checking Nodes";
            counter = 0;
            PercentProgress = 0;
            OnUpdated?.Invoke(this, null);
            
            foreach (var node in nodes)
            {
                try
                {
                    var cpu = await nodeService.GetCpuStats(node);
                    var freeMemory = await nodeService.GetMemoryStats(node);

                    if (cpu.Usage > RequiredCpu || freeMemory.Free < RequiredMemory)
                    {
                        var c = await nodeService.GetContainerStats(node);
                        var containers = c.Containers;
                        nodeContainers.Add(node, containers.ToArray());
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"Error fetching cleanup data from node {node.Id}");
                    Logger.Error(e);
                }

                counter++;
                CalculateAndUpdateProgress(counter, nodeCount);
                OnUpdated?.Invoke(this, null);
            }

            // Searching for servers we can actually stop because they have the cleanup tag
            // and determine which servers we have to check for an illegal mc server 
            var serversToCheck = new List<Server>();
            var serversToCheckForMc = new List<Server>();
            
            Status = "Checking found servers";
            counter = 0;
            PercentProgress = 0;
            OnUpdated?.Invoke(this, null);

            // Count every container for progress calculation
            var allContainers = 0;
            foreach (var array in nodeContainers)
            {
                allContainers += array.Value.Length;
            }
            
            foreach (var nodeContainer in nodeContainers)
            {
                try
                {
                    foreach (var container in nodeContainer.Value)
                    {
                        var server = servers.First(x => x.Uuid.ToString() == container.Name);
                        var tagsJson = imageRepo
                            .Get()
                            .First(x => x.Id == server.Image.Id).TagsJson;

                        var tags = JsonConvert.DeserializeObject<string[]>(tagsJson) ?? Array.Empty<string>();

                        if (tags.FirstOrDefault(x => x == "cleanup") != null)
                        {
                            serversToCheck.Add(server);
                        }

                        if (tags.FirstOrDefault(x => x == "illegalmc") != null)
                        {
                            serversToCheckForMc.Add(server);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"Error processing cleanup data from node {nodeContainer.Key.Id}");
                    Logger.Error(e);
                }
                
                counter++;
                CalculateAndUpdateProgress(counter, allContainers);
                OnUpdated?.Invoke(this, null);
            }

            // Now we gonna scan every tagged server
            Status = "Scanning servers";
            counter = 0;
            PercentProgress = 0;
            OnUpdated?.Invoke(this, null);
            
            foreach (var server in serversToCheck)
            {
                try
                {
                    var serverData = serverRepository
                        .Get()
                        .Include(x => x.MainAllocation)
                        .Include(x => x.Node)
                        .Include(x => x.Variables)
                        .First(x => x.Id == server.Id);
                    
                    var players = GetPlayers(serverData.Node, serverData.MainAllocation);
                    var stats = await serverService.GetDetails(server);
                    
                    var exception = exceptions.FirstOrDefault(x => x.ServerId == server.Id) != null;

                    if (stats != null)
                    {
                        if (exception)
                        {
                            if (players == 0 && stats.Utilization.Uptime > TimeSpan.FromHours(6).TotalMilliseconds)
                            {
                                await serverService.SetPowerState(server, PowerSignal.Restart);
                                ServersCleaned++;
                                OnUpdated?.Invoke(this, null);
                            }
                            else
                            {
                                ServersRunning++;
                                OnUpdated?.Invoke(this, null);
                            }
                        }
                        else
                        {
                            if (players == 0 && stats.Utilization.Uptime > TimeSpan.FromMinutes(10).TotalMilliseconds)
                            {
                                var cleanupVar = serverData.Variables.FirstOrDefault(x => x.Key == "J2S");

                                if (cleanupVar == null)
                                {
                                    await serverService.SetPowerState(server, PowerSignal.Stop);
                                    ServersCleaned++;
                                    OnUpdated?.Invoke(this, null);
                                }
                                else
                                {
                                    if (cleanupVar.Value == "1")
                                    {
                                        await serverService.SetPowerState(server, PowerSignal.Restart);
                                        ServersCleaned++;
                                        OnUpdated?.Invoke(this, null);
                                    }
                                    else
                                    {
                                        await serverService.SetPowerState(server, PowerSignal.Stop);
                                        ServersCleaned++;
                                        OnUpdated?.Invoke(this, null);
                                    }
                                }
                            }
                            else
                            {
                                ServersRunning++;
                                OnUpdated?.Invoke(this, null);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"Error scanning {server.Name}");
                    Logger.Error(e);
                }
                
                counter++;
                CalculateAndUpdateProgress(counter, serversToCheck.Count);
                OnUpdated?.Invoke(this, null);
            }
            
            // Finally we have to check all code container allocations
            // for illegal hosted mc servers
            
            Status = "Scanning code containers";
            counter = 0;
            PercentProgress = 0;
            OnUpdated?.Invoke(this, null);

            foreach (var server in serversToCheckForMc)
            {
                try
                {
                    var serverData = serverRepository
                        .Get()
                        .Include(x => x.Allocations)
                        .Include(x => x.Node)
                        .First(x => x.Id == server.Id);
                    
                    foreach (var allocation in serverData.Allocations)
                    {
                        if (GetPlayers(server.Node, allocation) != -1)
                        {
                            // TODO: Suspend server
                            Logger.Warn("Found CC running mc: https://moonlight.endelon-hosting.de/server/" +
                                        server.Uuid + "/");
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"Error scanning (cc) {server.Name}");
                    Logger.Error(e);
                }
                
                counter++;
                CalculateAndUpdateProgress(counter, serversToCheckForMc.Count);
                OnUpdated?.Invoke(this, null);
            }
            
            watch.Stop();
            
            Status = $"Cleanup finished. Duration: {Math.Round(TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalMinutes, 2)} Minutes";
            PercentProgress = 100;
            OnUpdated?.Invoke(this, null);
        }

        IsRunning = false;
        CompletedAt = DateTime.Now;
        CleanupsPerformed++;
        OnUpdated?.Invoke(this, null);
    }

    private int GetPlayers(Node node, NodeAllocation allocation)
    {
        var ms = new MineStat(node.Fqdn, (ushort)allocation.Port);

        if (ms.ServerUp)
        {
            return ms.CurrentPlayersInt;
        }
        else
        {
            return -1;
        }
    }

    private void CalculateAndUpdateProgress(int now, int all)
    {
        PercentProgress = (int)Math.Round((now / (double)all) * 100);
    }
}