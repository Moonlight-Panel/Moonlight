using Moonlight.Core.Models.Enums;

namespace Moonlight.Core.Models.Abstractions;

public class FlagStorage
{
    private readonly List<string> FlagList;

    public UserFlag[] Flags => FlagList
        .Select(x => Enum.Parse(typeof(UserFlag), x))
        .Select(x => (UserFlag)x)
        .ToArray();
    
    public string[] RawFlags => FlagList.ToArray();
    public string RawFlagString => string.Join(";", FlagList);

    public bool this[UserFlag flag]
    {
        get => Flags.Contains(flag);
        set => Set(flag.ToString(), value);
    }

    public bool this[string flagName]
    {
        get => FlagList.Contains(flagName);
        set => Set(flagName, value);
    }
    
    public FlagStorage(string flagString)
    {
        FlagList = flagString
            .Split(";")
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();
    }

    public void Set(string flagName, bool shouldAdd)
    {
        if (shouldAdd)
        {
            if(!FlagList.Contains(flagName))
                FlagList.Add(flagName);
        }
        else
        {
            if (FlagList.Contains(flagName))
                FlagList.Remove(flagName);
        }
    }
}