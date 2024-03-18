using MoonCore.Attributes;

namespace Moonlight.Core.Services;

[Scoped]
public class ScopedStorageService
{
    private readonly Dictionary<string, object> Data = new();

    public T? Get<T>(string key) where T : class
    {
        if (!Data.ContainsKey(key))
            return default;

        return Data[key] as T;
    }

    public void Set(string key, object data)
    {
        Data[key] = data;
    }
}