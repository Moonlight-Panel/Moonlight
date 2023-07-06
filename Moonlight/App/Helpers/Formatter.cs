using System.Text;
using Moonlight.App.Services;

namespace Moonlight.App.Helpers;

public static class Formatter
{
    public static string ReplaceEnd(string input, string substringToReplace, string newSubstring)
    {
        int lastIndexOfSubstring = input.LastIndexOf(substringToReplace);
        if (lastIndexOfSubstring >= 0)
        {
            input = input.Remove(lastIndexOfSubstring, substringToReplace.Length).Insert(lastIndexOfSubstring, newSubstring);
        }

        return input;
    }
    public static string ConvertCamelCaseToSpaces(string input)
    {
        StringBuilder output = new StringBuilder();

        foreach (char c in input)
        {
            if (char.IsUpper(c))
            {
                output.Append(' ');
            }

            output.Append(c);
        }

        return output.ToString().Trim();
    }
    public static string FormatUptime(double uptime)
    {
        TimeSpan t = TimeSpan.FromMilliseconds(uptime);

        if (t.Days > 0)
        {
            return $"{t.Days}d  {t.Hours}h {t.Minutes}m {t.Seconds}s";
        }
        else
        {
            return $"{t.Hours}h {t.Minutes}m {t.Seconds}s";
        }
    }
    
    public static string FormatUptime(TimeSpan t)
    {
        if (t.Days > 0)
        {
            return $"{t.Days}d  {t.Hours}h {t.Minutes}m {t.Seconds}s";
        }
        else
        {
            return $"{t.Hours}h {t.Minutes}m {t.Seconds}s";
        }
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

    public static string FormatAgoFromDateTime(DateTime dt, SmartTranslateService translateService = null)
    {
        TimeSpan timeSince = DateTime.UtcNow.Subtract(dt);

        if (timeSince.TotalMilliseconds < 1)
            return translateService == null ? "just now" : translateService.Translate("just now");

        if (timeSince.TotalMinutes < 1)
            return translateService == null ? "less than a minute ago" : translateService.Translate("less than a minute ago");

        if (timeSince.TotalMinutes < 2)
            return translateService == null ? "1 minute ago" : translateService.Translate("1 minute ago");

        if (timeSince.TotalMinutes < 60)
            return Math.Round(timeSince.TotalMinutes) + (translateService == null ? " minutes ago" : translateService.Translate(" minutes ago"));

        if (timeSince.TotalHours < 2)
            return translateService == null ? "1 hour ago" : translateService.Translate("1 hour ago");

        if (timeSince.TotalHours < 24)
            return Math.Round(timeSince.TotalHours) + (translateService == null ? " hours ago" : translateService.Translate(" hours ago"));

        if (timeSince.TotalDays < 2)
            return translateService == null ? "1 day ago" : translateService.Translate("1 day ago");

        return Math.Round(timeSince.TotalDays) + (translateService == null ? " days ago" : translateService.Translate(" days ago"));
    }
    
    public static string FormatDate(DateTime e)
    {
        string i2s(int i)
        {
            if (i.ToString().Length < 2)
                return "0" + i;
            return i.ToString();
        }
        
        return $"{i2s(e.Day)}.{i2s(e.Month)}.{e.Year} {i2s(e.Hour)}:{i2s(e.Minute)}";
    }
    
    public static string FormatDateOnly(DateTime e)
    {
        string i2s(int i)
        {
            if (i.ToString().Length < 2)
                return "0" + i;
            return i.ToString();
        }
        
        return $"{i2s(e.Day)}.{i2s(e.Month)}.{e.Year}";
    }

    public static string FormatSize(double bytes)
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
    
    public static double BytesToGb(long bytes)
    {
        const double gbMultiplier = 1024 * 1024 * 1024; // 1 GB = 1024 MB * 1024 KB * 1024 B

        double gigabytes = (double)bytes / gbMultiplier;
        return gigabytes;
    }
}