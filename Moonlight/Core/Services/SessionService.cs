using MoonCore.Attributes;
using Moonlight.Core.Models;

namespace Moonlight.Core.Services;

[Singleton]
public class SessionService
{
    public Session[] Sessions => Get().Result;
    
    private readonly List<Session> Data = new();

    public Task Add(Session session)
    {
        lock (Data)
            Data.Add(session);
        
        return Task.CompletedTask;
    }

    public Task Remove(Session session)
    {
        lock (Data)
        {
            if (Data.Contains(session))
                Data.Remove(session);
        }
        
        return Task.CompletedTask;
    }

    public Task<Session[]> Get()
    {
        lock (Data)
            return Task.FromResult(Data.ToArray());
    }
}