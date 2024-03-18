﻿using System.IO.Compression;
using MoonCore.Attributes;
using MoonCore.Helpers;
using Moonlight.Core.Extensions;
using Moonlight.Core.Interfaces;

namespace Moonlight.Core.Services;

[Singleton]
public class DiagnoseService
{
    private readonly PluginService PluginService;
    private readonly IServiceProvider ServiceProvider;

    public DiagnoseService(PluginService pluginService, IServiceProvider serviceProvider)
    {
        PluginService = pluginService;
        ServiceProvider = serviceProvider;
    }

    public async Task<byte[]> GenerateReport()
    {
        using var scope = ServiceProvider.CreateScope();
        
        using var dataStream = new MemoryStream();
        var zipArchive = new ZipArchive(dataStream, ZipArchiveMode.Create, true);

        await PluginService.ExecuteFuncAsync<IDiagnoseAction>(
            async x => await x.GenerateReport(zipArchive, scope.ServiceProvider)
        );

        await zipArchive.AddText("exported_at.txt", Formatter.FormatDate(DateTime.UtcNow));
        
        zipArchive.Dispose();
        
        return dataStream.ToArray();
    }
}