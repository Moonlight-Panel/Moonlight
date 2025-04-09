using Hangfire;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Extended.PermFilter;
using Moonlight.Shared.Http.Responses.Admin.Hangfire;

namespace Moonlight.ApiServer.Http.Controllers.Admin.Sys;

[ApiController]
[Route("api/admin/system/hangfire")]
[RequirePermission("admin.system.hangfire")]
public class HangfireController : Controller
{
    private readonly JobStorage JobStorage;

    public HangfireController(JobStorage jobStorage)
    {
        JobStorage = jobStorage;
    }

    [HttpGet("stats")]
    public Task<HangfireStatsResponse> GetStats()
    {
        var statistics = JobStorage.GetMonitoringApi().GetStatistics();

        return Task.FromResult(new HangfireStatsResponse()
        {
            Awaiting = statistics.Awaiting,
            Deleted = statistics.Deleted,
            Enqueued = statistics.Enqueued,
            Failed = statistics.Failed,
            Processing = statistics.Processing,
            Queues = statistics.Queues,
            Recurring = statistics.Recurring,
            Retries = statistics.Retries,
            Scheduled = statistics.Scheduled,
            Servers = statistics.Servers,
            Succeeded = statistics.Succeeded
        });
    }
}