using Logging.Net;

namespace Moonlight.App.Helpers;

public static class ParseHelper
{
    public static int MinecraftToInt(string raw)
    {
        var versionWithoutPre = raw.Split("_")[0];
        versionWithoutPre = versionWithoutPre.Split("-")[0];

        // Fuck you 1.7.10 ;)
        versionWithoutPre = versionWithoutPre.Replace("1.7.10", "1.7");

        if (versionWithoutPre.Count(x => x == "."[0]) == 1)
            versionWithoutPre += ".0";

        var x = versionWithoutPre.Replace(".", "");

        return int.Parse(x);
    }

    public static string FirstPartStartingWithNumber(string raw)
    {
        var numbers = "0123456789";
        var res = "";
        var found = false;

        foreach (var p in raw)
        {
            if (!found)
                found = numbers.Contains(p);

            if (found)
                res += p;
        }

        return res;
    }
}