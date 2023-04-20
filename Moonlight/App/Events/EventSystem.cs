using System.Diagnostics;
using Logging.Net;

namespace Moonlight.App.Events;

public class EventSystem
{
    private Dictionary<int, object> Storage = new();
    private List<Subscriber> Subscribers = new();

    private readonly bool Debug = true;
    private readonly bool DisableWarning = false;
    private readonly TimeSpan TookToLongTime = TimeSpan.FromSeconds(1);

    public Task On<T>(string id, object handle, Func<T, Task> action)
    {
        if(Debug)
            Logger.Debug($"{handle} subscribed to '{id}'");
        
        lock (Subscribers)
        {
            if (!Subscribers.Any(x => x.Id == id && x.Handle == handle))
            {
                Subscribers.Add(new ()
                {
                    Action = action,
                    Handle = handle,
                    Id = id
                });
            }
        }
        
        return Task.CompletedTask;
    }

    public Task Emit(string id, object? d = null)
    {
        int hashCode = -1;

        if (d != null)
        {
            hashCode = d.GetHashCode();
            Storage.TryAdd(hashCode, d);
        }

        Subscriber[] subscribers;

        lock (Subscribers)
        {
            subscribers = Subscribers
                .Where(x => x.Id == id)
                .ToArray();
        }

        var tasks = new List<Task>();

        foreach (var subscriber in subscribers)
        {
            tasks.Add(new Task(() =>
            {
                int storageId = hashCode + 0; // To create a copy of the hash code

                object? data = null;

                if (storageId != -1)
                {
                    if (Storage.TryGetValue(storageId, out var value))
                    {
                        data = value;
                    }
                    else
                    {
                        Logger.Warn($"Object with the hash '{storageId}' was not present in the storage");
                        return;
                    }
                }

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var del = (Delegate)subscriber.Action;

                try
                {
                    ((Task)del.DynamicInvoke(data)!).Wait();
                }
                catch (Exception e)
                {
                    Logger.Warn($"Error emitting '{subscriber.Id} on {subscriber.Handle}'");
                    Logger.Warn(e);
                }

                stopWatch.Stop();

                if (!DisableWarning)
                {
                    if (stopWatch.Elapsed.TotalMilliseconds > TookToLongTime.TotalMilliseconds)
                    {
                        Logger.Warn($"Subscriber {subscriber.Handle} for event '{subscriber.Id}' took long to process. {stopWatch.Elapsed.TotalMilliseconds}ms");
                    }
                }

                if (Debug)
                {
                    Logger.Debug($"Subscriber {subscriber.Handle} for event '{subscriber.Id}' took {stopWatch.Elapsed.TotalMilliseconds}ms");
                }
            }));
        }

        foreach (var task in tasks)
        {
            task.Start();
        }

        Task.Run(() =>
        {
            Task.WaitAll(tasks.ToArray());
            Storage.Remove(hashCode);
            Logger.Debug($"Completed all event tasks for '{id}' and removed object from storage");
        });
        
        if(Debug)
            Logger.Debug($"Completed event emit '{id}'");
        
        return Task.CompletedTask;
    }

    public Task Off(string id, object handle)
    {
        if(Debug)
            Logger.Debug($"{handle} unsubscribed to '{id}'");
        
        lock (Subscribers)
        {
            Subscribers.RemoveAll(x => x.Id == id && x.Handle == handle);
        }
        
        return Task.CompletedTask;
    }
}