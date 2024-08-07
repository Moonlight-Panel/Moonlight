using MoonCore.Attributes;

namespace Moonlight.Client.App.Services;

[Singleton]
public class EventService
{
    private readonly Dictionary<string, List<object>> Handlers = new();
    private readonly ILogger<EventService> Logger;

    public EventService(ILogger<EventService> logger)
    {
        Logger = logger;
    }

    public async Task Invoke(string name, params object[] data)
    {
        Type type;
        object[] handlers;

        lock (Handlers)
        {
            if(!Handlers.ContainsKey(name))
                return;

            handlers = Handlers[name].ToArray();
        }

        switch (data.Length)
        {
            default:
                type = typeof(Func<Task>);
                break;
            
            case 1:
                type = typeof(Func<,>).MakeGenericType(data[0].GetType(), typeof(Task));
                break;
            
            case 2:
                type = typeof(Func<,,>).MakeGenericType(data[0].GetType(), data[1].GetType(), typeof(Task));
                break;
            
            case 3:
                type = typeof(Func<,,,>).MakeGenericType(data[0].GetType(), data[1].GetType(), data[2].GetType(), typeof(Task));
                break;
            
            case 4:
                type = typeof(Func<,,,,>).MakeGenericType(data[0].GetType(), data[1].GetType(), data[2].GetType(), data[3].GetType(), typeof(Task));
                break;
        }

        foreach (var handler in handlers)
        {
            try
            {
                var task = (Task)type.GetMethod("Invoke")!.Invoke(handler, data)!;
                await task;
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured during calling event handler: {e}", e);
            }
        }
    }

    public void Remove(string name, int id)
    {
        lock (Handlers)
        {
            if(!Handlers.ContainsKey(name))
                return;

            Handlers[name].RemoveAll(x => x.GetHashCode() == id);
        }
    }

    private int OnRaw(string name, object func)
    {
        lock (Handlers)
        {
            if (!Handlers.ContainsKey(name))
                Handlers.Add(name, new());

            Handlers[name].Add(func);
        }
        
        return func.GetHashCode();
    }

    public int On(string name, Func<Task> func) => OnRaw(name, func);
    public int On<T>(string name, Func<T, Task> func) => OnRaw(name, func);
    public int On<T1, T2>(string name, Func<T1, T2, Task> func) => OnRaw(name, func);
    public int On<T1, T2, T3>(string name, Func<T1, T2, T3, Task> func) => OnRaw(name, func);
    public int On<T1, T2, T3, T4>(string name, Func<T1, T2, T3, T4, Task> func) => OnRaw(name, func);
}