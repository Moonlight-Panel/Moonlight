namespace Moonlight.Core.Extensions;

public static class EventHandlerExtensions
{
    public static async Task InvokeAsync(this EventHandler handler)
    {
        var tasks = handler
            .GetInvocationList()
            .Select(x => new Task(() => x.DynamicInvoke(null, null)))
            .ToArray();

        foreach (var task in tasks)
        {
            task.Start();
        }

        await Task.WhenAll(tasks);
    }

    public static async Task InvokeAsync<T>(this EventHandler<T>? handler, T? data = default(T))
    {
        if(handler == null)
            return;
        
        var tasks = handler
            .GetInvocationList()
            .Select(x => new Task(() => x.DynamicInvoke(null, data)))
            .ToArray();

        foreach (var task in tasks)
        {
            task.Start();
        }

        await Task.WhenAll(tasks);
    }
}