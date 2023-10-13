using Moonlight.App.Models.Abstractions;

namespace Moonlight.App.Services;

public class SessionService
{
    private readonly List<Session> AllSessions = new();

    public Session[] Sessions => GetSessions();

    public Task Register(Session session)
    {
        lock (AllSessions)
        {
            AllSessions.Add(session);
        }
        
        return Task.CompletedTask;
    }

    public Task Unregister(Session session)
    {
        lock (AllSessions)
        {
            AllSessions.Remove(session);
        }
        
        return Task.CompletedTask;
    }

    public Session[] GetSessions()
    {
        lock (AllSessions)
        {
            return AllSessions.ToArray();
        }
    }
}