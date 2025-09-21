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
    public async Task<ActionResult<ICountedData<ThemeResponse>>> GetAsync(
        [FromQuery] int startIndex,
        [FromQuery] int count,
        [FromQuery] string? orderBy,
        [FromQuery] string? filter,
        [FromQuery] string orderByDir = "asc"
    )
    {
        if (count > 100)
            return Problem("You cannot fetch more items than 100 at a time", statusCode: 400);
        
        IQueryable<Theme> query = ThemeRepository.Get();

        query = orderBy switch
        {
            nameof(Theme.Id) => orderByDir == "desc"
                ? query.OrderByDescending(x => x.Id)
                : query.OrderBy(x => x.Id),
                
            nameof(Theme.Name) => orderByDir == "desc"
                ? query.OrderByDescending(x => x.Name)
                : query.OrderBy(x => x.Name),
            
            nameof(Theme.Version) => orderByDir == "desc"
                ? query.OrderByDescending(x => x.Version)
                : query.OrderBy(x => x.Version),
            
            _ => query.OrderBy(x => x.Id)
        };

        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(x =>
                EF.Functions.ILike(x.Name, $"%{filter}%")
            );
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(startIndex)
            .Take(count)
            .AsNoTracking()
            .ProjectToResponse()
            .ToArrayAsync();

        return new CountedData<ThemeResponse>()
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "permissions:admin.system.customisation.themes.read")]
    public async Task<ActionResult<ThemeResponse>> GetSingleAsync([FromRoute] int id)
    {
        var theme = await ThemeRepository
            .Get()
            .AsNoTracking()
            .ProjectToResponse()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (theme == null)
            return Problem("Theme with this id not found", statusCode: 404);

        return theme;
    }

    [HttpPost]
    [Authorize(Policy = "permissions:admin.system.customisation.themes.write")]
    public async Task<ActionResult<ThemeResponse>> CreateAsync([FromBody] CreateThemeRequest request)
    {
        var theme = ThemeMapper.ToTheme(request);

        var finalTheme = await ThemeRepository.AddAsync(theme);

        return ThemeMapper.ToResponse(finalTheme);
    }

    [HttpPatch("{id:int}")]
    [Authorize(Policy = "permissions:admin.system.customisation.themes.write")]
    public async Task<ActionResult<ThemeResponse>> UpdateAsync([FromRoute] int id, [FromBody] UpdateThemeRequest request)
    {
        var theme = await ThemeRepository
            .Get()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (theme == null)
            return Problem("Theme with this id not found", statusCode: 404);

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

            await ThemeRepository.RunTransactionAsync(set => { set.UpdateRange(otherThemes); });
        }

        ThemeMapper.Merge(theme, request);
        
        await ThemeRepository.UpdateAsync(theme);

        return ThemeMapper.ToResponse(theme);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "permissions:admin.system.customisation.themes.write")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int id)
    {
        var theme = await ThemeRepository
            .Get()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (theme == null)
            return Problem("Theme with this id not found", statusCode: 404);

        await ThemeRepository.RemoveAsync(theme);
        return NoContent();
    }
}