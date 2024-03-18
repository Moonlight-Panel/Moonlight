using System.Diagnostics;
using System.IO.Compression;
using MoonCore.Abstractions;
using Moonlight.Core.Extensions;
using Moonlight.Core.Interfaces;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Services;
using Newtonsoft.Json;

namespace Moonlight.Features.Servers.Implementations.Diagnose;

public class NodesDiagnoseAction : IDiagnoseAction
{
    public async Task GenerateReport(ZipArchive archive, IServiceProvider serviceProvider)
    {
        var nodeRepo = serviceProvider.GetRequiredService<Repository<ServerNode>>();
        var nodeService = serviceProvider.GetRequiredService<NodeService>();

        foreach (var node in nodeRepo.Get().ToArray())
        {
            var nodeJson = JsonConvert.SerializeObject(node);
            var nodeCopy = JsonConvert.DeserializeObject<ServerNode>(nodeJson)!;

            nodeCopy.Token = "censored";

            await archive.AddText($"servers/nodes/{node.Id}/config.json", JsonConvert.SerializeObject(nodeCopy, Formatting.Indented));

            try
            {
                var logs = await nodeService.GetLogs(node);
                await archive.AddText($"servers/nodes/{node.Id}/logs.txt", logs);
            }
            catch (Exception e)
            {
                await archive.AddText($"servers/nodes/{node.Id}/logs.txt", e.ToStringDemystified());
            }

            try
            {
                var status = await nodeService.GetStatus(node);
                await archive.AddText($"servers/nodes/{node.Id}/status.json", JsonConvert.SerializeObject(status, Formatting.Indented));
            }
            catch (Exception e)
            {
                await archive.AddText($"servers/nodes/{node.Id}/status.txt", e.ToStringDemystified());
            }
        }
    }
}