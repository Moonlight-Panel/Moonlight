namespace Moonlight.App.Helpers;

public static class ParseHelper
{
    public static int MinecraftToInt(string raw)
    {
        var versionWithoutPre = raw.Split("-")[0];

        if (versionWithoutPre.Count(x => x == "."[0]) == 1)
            versionWithoutPre += ".0";

        return int.Parse(versionWithoutPre.Replace(".", ""));
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