using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoonCore.Exceptions;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Models;
using MoonCore.Models;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Mappers;
using Moonlight.Shared.Http.Requests.Admin.Sys.Theme;
using Moonlight.Shared.Http.Responses.Admin;

namespace Moonlight.ApiServer.Http.Controllers.Admin.Sys.Customisation;

[ApiController]
[Route("api/admin/system/customisation/themes")]
public class ThemesController : Controller
{
    private readonly DatabaseRepository<Theme> ThemeRepository;

    public ThemesController(DatabaseRepository<Theme> themeRepository)
    {
        ThemeRepository = themeRepository;
    }

    [HttpGet]
    [Authorize(Policy = "permissions:admin.system.customisation.themes.read")]
    public async Task<PagedData<ThemeResponse>> Get([FromQuery] PagedOptions options)
    {
        var count = await ThemeRepository.Get().CountAsync();
        
        var items = await ThemeRepository
            .Get()
            .Skip(options.Page * options.PageSize)
            .Take(options.PageSize)
            .ToArrayAsync();
        
        var mappedItems = items
            .Select(ThemeMapper.ToResponse)
            .ToArray();
        
        return new PagedData<ThemeResponse>()
        {
            CurrentPage = options.Page,
            Items = mappedItems,
            PageSize = options.PageSize,
            TotalItems = count,
            TotalPages = count == 0 ? 0 : (count - 1) / options.PageSize
        };
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "permissions:admin.system.customisation.themes.read")]
    public async Task<ThemeResponse> GetSingle([FromRoute] int id)
    {
        var theme = await ThemeRepository
            .Get()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (theme == null)
            throw new HttpApiException("Theme with this id not found", 404);
        
        return ThemeMapper.ToResponse(theme);
    }

    [HttpPost]
    [Authorize(Policy = "permissions:admin.system.customisation.themes.write")]
    public async Task<ThemeResponse> Create([FromBody] CreateThemeRequest request)
    {
        var theme = ThemeMapper.ToTheme(request);
        
        var finalTheme = await ThemeRepository.Add(theme);
        
        return ThemeMapper.ToResponse(finalTheme);
    }

    [HttpPatch("{id:int}")]
    [Authorize(Policy = "permissions:admin.system.customisation.themes.write")]
    public async Task<ThemeResponse> Update([FromRoute] int id, [FromBody] UpdateThemeRequest request)
    {
        var theme = await ThemeRepository
            .Get()
            .FirstOrDefaultAsync(t => t.Id == id);
        
        if (theme == null)
            throw new HttpApiException("Theme with this id not found", 404);

        // Disable all other enabled themes if we are enabling the current theme.
        // This ensures only one theme is enabled at the time
        if (request.IsEnabled)
        {
            var otherThemes = await ThemeRepository
                .Get()
                .Where(x => x.IsEnabled && x.Id != id)
                .ToArrayAsync();

            foreach (var otherTheme in otherThemes)
                otherTheme.IsEnabled = false;

            await ThemeRepository.RunTransaction(set =>
            {
                set.UpdateRange(otherThemes);
            });
        }
        
        ThemeMapper.Merge(theme, request);
        await ThemeRepository.Update(theme);
        
        return ThemeMapper.ToResponse(theme);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "permissions:admin.system.customisation.themes.write")]
    public async Task Delete([FromRoute] int id)
    {
        var theme = await ThemeRepository
            .Get()
            .FirstOrDefaultAsync(x => x.Id == id);
        
        if (theme == null)
            throw new HttpApiException("Theme with this id not found", 404);
        
        await ThemeRepository.Remove(theme);
    }
}