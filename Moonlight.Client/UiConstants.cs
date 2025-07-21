namespace Moonlight.Client;

public static class UiConstants
{
    public static readonly string[] AdminNavNames =
    [
        "Overview", "Customisation", "Files", "Hangfire", "Advanced", "Diagnose"
    ];

    public static readonly string[] AdminNavLinks =
    [
        "/admin/system", "/admin/system/customisation", "/admin/system/files", "/admin/system/hangfire",
        "/admin/system/advanced", "/admin/system/diagnose"
    ];
}