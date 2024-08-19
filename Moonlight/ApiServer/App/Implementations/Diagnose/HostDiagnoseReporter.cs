using System.IO.Compression;
using MoonCore.Helpers;
using Moonlight.ApiServer.App.Extensions;
using Moonlight.ApiServer.App.Helpers;
using Moonlight.ApiServer.App.Interfaces;

namespace Moonlight.ApiServer.App.Implementations.Diagnose;

public class HostDiagnoseReporter : IDiagnoseReporter
{
    public async Task GenerateReport(ZipArchive archive, IServiceProvider provider)
    {
        var hostHelper = provider.GetRequiredService<HostHelper>();

        var text = "";

        text += $"Operating system: {await hostHelper.GetOsName()}\n";
        text += $"Moonlight uptime: {Formatter.FormatUptime(await hostHelper.GetUptime())}\n";
        text += $"CPU usage: {await hostHelper.GetCpuUsage()}\n";
        text += $"Memory usage: {Formatter.FormatSize(await hostHelper.GetMemoryUsage())}\n";

        await archive.AddText("host-info.txt", text);
    }
}