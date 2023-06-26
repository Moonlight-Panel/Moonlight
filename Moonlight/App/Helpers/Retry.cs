namespace Moonlight.App.Helpers;

public class Retry
{
    private List<Type> RetryExceptionTypes;
    private List<Func<Exception, bool>> RetryFilters;
    private int RetryTimes = 1;

    public Retry()
    {
        RetryExceptionTypes = new();
        RetryFilters = new();
    }

    public Retry Times(int times)
    {
        RetryTimes = times;
        return this;
    }

    public Retry At(Func<Exception, bool> filter)
    {
        RetryFilters.Add(filter);
        return this;
    }

    public Retry At<T>()
    {
        RetryExceptionTypes.Add(typeof(T));
        return this;
    }

    public async Task Call(Func<Task> method)
    {
        int triesLeft = RetryTimes;
        
        do
        {
            try
            {
                await method.Invoke();
                return;
            }
            catch (Exception e)
            {
                if(triesLeft < 1) // Throw if no tries left
                    throw;
                
                if (RetryExceptionTypes.Any(x => x.FullName == e.GetType().FullName))
                {
                    triesLeft--;
                    continue;
                }

                if (RetryFilters.Any(x => x.Invoke(e)))
                {
                    triesLeft--;
                    continue;
                }

                // Throw if not filtered -> unknown/unhandled
                throw;
            }
        } while (triesLeft >= 0);
    }
}