using Moonlight.App.Database.Entities;
using Moonlight.App.Models.Misc;
using Moonlight.App.Repositories;
using Moonlight.App.Services.Sessions;
using Newtonsoft.Json;

namespace Moonlight.App.Services;

public class AuditLogService
{
    private readonly AuditLogRepository AuditLogRepository;
    private readonly IdentityService IdentityService;

    public AuditLogService(AuditLogRepository auditLogRepository, IdentityService identityService)
    {
        AuditLogRepository = auditLogRepository;
        IdentityService = identityService;
    }

    public void Log(AuditLogType type, object data)
    {
        AuditLogRepository.Add(new()
        {
            System = true,
            Type = type,
            JsonData = JsonConvert.SerializeObject(data),
            Ip = IdentityService.GetIp()
        });
    }
}