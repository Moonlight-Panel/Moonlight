namespace Moonlight.Client;

public static class UiConstants
{
    public static readonly string[] AdminNavNames =
    [
        "Overview", "Theme", "Files", "Hangfire", "Advanced", "Diagnose"
    ];

    public static readonly string[] AdminNavLinks =
    [
        "/admin/system", "/admin/system/theme", "/admin/system/files", "/admin/system/hangfire",
        "/admin/system/advanced", "/admin/system/diagnose"
    ];
}