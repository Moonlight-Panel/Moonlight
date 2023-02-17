namespace Moonlight.App.Helpers;

public static class Formatter
{
    public static string FormatUptime(double uptime)
    {
        TimeSpan t = TimeSpan.FromMilliseconds(uptime);

        return $"{t.Hours}h {t.Minutes}m {t.Seconds}s";
    }

    private static double Round(this double d, int decimals)
    {
        return Math.Round(d, decimals);
    }

    public static string FormatSize(long bytes)
    {
        var i = Math.Abs(bytes) / 1024D;
        if (i < 1)
        {
            return bytes + " B";
        }
        else if (i / 1024D < 1)
        {
            return i.Round(2) + " KB";
        }
        else if (i / (1024D * 1024D) < 1)
        {
            return (i / 1024D).Round(2) + " MB";
        }
        else
        {
            return (i / (1024D * 1024D)).Round(2) + " GB";
        }
    }
}