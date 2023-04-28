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
    
    public static string GetHighestVersion(string[] versions)
    {
        // Initialize the highest version to the first version in the array
        string highestVersion = versions[0];

        // Loop through the remaining versions in the array
        for (int i = 1; i < versions.Length; i++)
        {
            // Compare the current version to the highest version
            if (CompareVersions(versions[i], highestVersion) > 0)
            {
                // If the current version is higher, update the highest version
                highestVersion = versions[i];
            }
        }

        return highestVersion;
    }

    public static int CompareVersions(string version1, string version2)
    {
        // Split the versions into their component parts
        string[] version1Parts = version1.Split('.');
        string[] version2Parts = version2.Split('.');

        // Compare each component part in turn
        for (int i = 0; i < version1Parts.Length && i < version2Parts.Length; i++)
        {
            int part1 = int.Parse(version1Parts[i]);
            int part2 = int.Parse(version2Parts[i]);

            if (part1 < part2)
            {
                return -1;
            }
            else if (part1 > part2)
            {
                return 1;
            }
        }

        // If we get here, the versions are equal up to the length of the shorter one.
        // If one version has more parts than the other, the longer one is considered higher.
        if (version1Parts.Length < version2Parts.Length)
        {
            return -1;
        }
        else if (version1Parts.Length > version2Parts.Length)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}