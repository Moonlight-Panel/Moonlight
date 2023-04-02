namespace Moonlight.App.Models.Misc;

public enum AuditLogType
{
    Login,
    Register,
    ChangePassword,
    ChangePowerState,
    CreateBackup,
    RestoreBackup,
    DeleteBackup,
    DownloadBackup,
    CreateServer,
    ReinstallServer,
    CancelSubscription,
    ApplySubscriptionCode,
    EnableTotp,
    DisableTotp,
    AddDomainRecord,
    UpdateDomainRecord,
    DeleteDomainRecord,
    PasswordReset,
    CleanupEnabled,
    CleanupDisabled,
    CleanupTriggered,
}