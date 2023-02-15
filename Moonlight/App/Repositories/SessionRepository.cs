using Moonlight.App.Models.Misc;

namespace Moonlight.App.Repositories;

public class SessionRepository
{
    private readonly List<Session> Sessions;

    public SessionRepository()
    {
        Sessions = new();
    }

    public Session[] Get()
    {
        lock (Sessions)
        {
            return Sessions.ToArray();
        }
    }

    public void Add(Session session)
    {
        lock (Sessions)
        {
            Sessions.Add(session);
        }
    }

    public void Delete(Session session)
    {
        lock (Sessions)
        {
            Sessions.Remove(session);
        }
    }
}