using System.Diagnostics;
using Logging.Net;

namespace Moonlight.App.Events;

public class EventSystem
{
    private List<Subscriber> Subscribers = new();

    private readonly bool Debug = false;
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

    public Task Emit(string id, object? data = null)
    {
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

            if(Debug)
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
    
    public Task<T> WaitForEvent<T>(string id, object handle, Func<T, bool> filter)
    {
        var taskCompletionSource = new TaskCompletionSource<T>();
    
        Func<T, Task> action = async data =>
        {
            if (filter.Invoke(data))
            {
                taskCompletionSource.SetResult(data);
                await Off(id, handle);
            }
        };

        On<T>(id, handle, action);

        return taskCompletionSource.Task;
    }
}