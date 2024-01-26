using Mappy.Net;
using Moonlight.Core.Models.Abstractions;
using Moonlight.Core.Repositories;
using Moonlight.Core.Services;
using Moonlight.Features.Theming.Entities;
using Moonlight.Features.Theming.Models.Abstractions;

namespace Moonlight.Features.Theming.Services;

public class ThemeService
{
    private readonly IServiceProvider ServiceProvider;
    private readonly ConfigService ConfigService;

    public ThemeService(IServiceProvider serviceProvider, ConfigService configService)
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