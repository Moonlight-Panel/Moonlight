namespace Moonlight.Core.Helpers;

public static class PathBuilder
{
    public static string Dir(params string[] parts)
    {
        var res = "";

        foreach (var part in parts)
        {
            res += part + Path.DirectorySeparatorChar;
        }

        return res.Replace(
            $"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", 
            $"{Path.DirectorySeparatorChar}"
        );
    }
    
    public static string File(params string[] parts)
    {
        var res = "";

        foreach (var part in parts)
        {
            res += part + (part == parts.Last() ? "" : Path.DirectorySeparatorChar);
        }

        return res.Replace(
            $"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", 
            $"{Path.DirectorySeparatorChar}"
        );
    }
}