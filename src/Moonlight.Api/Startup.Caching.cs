using Moonlight.Api.Infrastructure.Caching;

namespace Moonlight.Api;

public static partial class Startup
{
    private static void AddCaching(WebApplicationBuilder builder)
    {
        var cacheOptions = builder.Configuration
            .GetSection("Moonlight:Caching")
            .Get<CacheOptions>() ?? new CacheOptions();

        if (string.IsNullOrWhiteSpace(cacheOptions.RedisUrl))
            builder.Services.AddDistributedMemoryCache();
        else
        {
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheOptions.RedisUrl;
            });
        }

        builder.Services.AddMemoryCache();
        builder.Services.AddHybridCache();
    }
}