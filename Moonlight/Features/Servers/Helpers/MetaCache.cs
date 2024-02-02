namespace Moonlight.Features.Servers.Helpers;

public class MetaCache<T>
{
    private readonly Dictionary<int, T> Cache = new();

    public Task Update(int id, Action<T> metaAction)
    {
        lock (Cache)
        {
            T? meta = default;

            if (Cache.ContainsKey(id))
                meta = Cache[id];

            if (meta == null)
            {
                meta = Activator.CreateInstance<T>();
                Cache.Add(id, meta);
            }
            
            metaAction.Invoke(meta);
        }
        
        return Task.CompletedTask;
    }

    public Task<T> Get(int id)
    {
        lock (Cache)
        {
            if(!Cache.ContainsKey(id))
                Cache.Add(id, Activator.CreateInstance<T>());

            return Task.FromResult(Cache[id]);
        }
    }
}