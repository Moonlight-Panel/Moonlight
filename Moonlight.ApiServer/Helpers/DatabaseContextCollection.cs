using System.Collections;

namespace Moonlight.ApiServer.Helpers;

public class DatabaseContextCollection : IEnumerable<Type>
{
    private readonly List<Type> Types = new();

    public IEnumerator<Type> GetEnumerator() => Types.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Types.GetEnumerator();

    public void Add<T>() where T : DatabaseContext
        => Types.Add(typeof(T));

    public void Remove(Type type) => Types.Remove(type);
    public void Remove<T>() where T : DatabaseContext => Remove(typeof(T));
}