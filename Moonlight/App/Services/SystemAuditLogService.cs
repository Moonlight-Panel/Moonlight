using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories;
using Newtonsoft.Json;

namespace Moonlight.App.Services;

public class SystemAuditLogService
{
    private readonly AuditLogRepository AuditLogRepository;

    public SystemAuditLogService(AuditLogRepository auditLogRepository)
    {
        AuditLogRepository = auditLogRepository;
    }

    public void Log(AuditLogType type, object data)
    {
        AuditLogRepository.Add(new()
        {
            System = true,
            Type = type,
            JsonData = JsonConvert.SerializeObject(data)
        });
    }
}