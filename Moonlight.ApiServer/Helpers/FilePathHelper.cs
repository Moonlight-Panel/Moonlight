namespace Moonlight.ApiServer.Helpers;

public class FilePathHelper
{
    public static string SanitizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        // Normalize separators
        path = path.Replace('\\', '/');

        // Remove ".." and "."
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(part => part != ".." && part != ".");

        var sanitized = string.Join("/", parts);

        // Ensure it does not start with a slash
        if (sanitized.StartsWith('/'))
            sanitized = sanitized.TrimStart('/');

        return sanitized;
    }
}