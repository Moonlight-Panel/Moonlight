using Mappy.Net;
using Moonlight.Core.Database.Entities;
using Moonlight.Core.Models.Abstractions;
using Moonlight.Core.Repositories;

namespace Moonlight.Core.Services.Sys;

public class MoonlightThemeService
{
    private readonly IServiceProvider ServiceProvider;
    private readonly ConfigService ConfigService;

    public MoonlightThemeService(IServiceProvider serviceProvider, ConfigService configService)
    {
        ServiceProvider = serviceProvider;
        ConfigService = configService;
    }

    public Task<ApplicationTheme[]> GetInstalled()
    {
        using var scope = ServiceProvider.CreateScope();
        var themeRepo = scope.ServiceProvider.GetRequiredService<Repository<Theme>>();

        var themes = new List<ApplicationTheme>();
        
        themes.AddRange(themeRepo
            .Get()
            .ToArray()
            .Select(x => Mapper.Map<ApplicationTheme>(x)));

        if (ConfigService.Get().Theme.EnableDefault)
        {
            themes.Insert(0, new()
            {
                Id = 0,
                Name = "Moonlight Default",
                Author = "MasuOwO",
                Enabled = true,
                CssUrl = "/css/theme.css",
                DonateUrl = "https://ko-fi.com/masuowo"
            });
        }

        return Task.FromResult(themes.ToArray());
    }

    public async Task<ApplicationTheme[]> GetEnabled() => 
        (await GetInstalled())
        .Where(x => x.Enabled)
        .ToArray();
}