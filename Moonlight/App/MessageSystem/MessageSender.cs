using System.Diagnostics;
using Logging.Net;

namespace Moonlight.App.MessageSystem;

public class MessageSender
{
    private readonly List<MessageSubscriber> Subscribers;
    
    public bool Debug { get; set; }
    public TimeSpan TookToLongTime { get; set; } = TimeSpan.FromSeconds(1);

    public MessageSender()
    {
        Subscribers = new();
    }

    public void Subscribe<T, K>(string name, object bind, Func<K, Task> method)
    {
        lock (Subscribers)
        {
            Subscribers.Add(new ()
            {
                Name = name,
                Action = method,
                Type = typeof(T),
                Bind = bind
            });
        }
        
        if(Debug)
            Logger.Debug($"{bind} subscribed to '{name}'");
    }

    public void Unsubscribe(string name, object bind)
    {
        lock (Subscribers)
        {
            Subscribers.RemoveAll(x => x.Bind == bind);
        }
        
        if(Debug)
            Logger.Debug($"{bind} unsubscribed from '{name}'");
    }

    public Task Emit(string name, object? value, bool disableWarning = false)
    {
        lock (Subscribers)
        {
            foreach (var subscriber in Subscribers)
            {
                if (subscriber.Name == name)
                {
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();

                    var del = (Delegate)subscriber.Action;

                    ((Task)del.DynamicInvoke(value)!).Wait();

                    stopWatch.Stop();

                    if (!disableWarning)
                    {
                        if (stopWatch.Elapsed.TotalMilliseconds > TookToLongTime.TotalMilliseconds)
                        {
                            Logger.Warn(
                                $"Subscriber {subscriber.Type.Name} for event '{name}' took long to process. {stopWatch.Elapsed.TotalMilliseconds}ms");
                        }
                    }

                    if (Debug)
                    {
                        Logger.Debug(
                            $"Subscriber {subscriber.Type.Name} for event '{name}' took {stopWatch.Elapsed.TotalMilliseconds}ms");
                    }
                }
            }
        }
        
        return Task.CompletedTask;
    }
}