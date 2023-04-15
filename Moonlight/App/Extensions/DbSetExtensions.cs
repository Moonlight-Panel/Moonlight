using Microsoft.EntityFrameworkCore;

namespace Moonlight.App.Extensions;

public static class DbSetExtensions
{
    public static T Random<T>(this DbSet<T> repo) where T : class
    {
        Random rand = new Random();
        int toSkip = rand.Next(0, repo.Count());

        return repo.Skip(toSkip).Take(1).First();
    }
}