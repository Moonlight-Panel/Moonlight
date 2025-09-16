using Moonlight.ApiServer.Database.Entities;
using Moonlight.Shared.Http.Requests.Admin.Sys.Theme;
using Moonlight.Shared.Http.Responses.Admin;
using Riok.Mapperly.Abstractions;

namespace Moonlight.ApiServer.Mappers;

[Mapper]
public static partial class ThemeMapper
{
    // Mappers
    public static partial ThemeResponse ToResponse(Theme theme);
    public static partial Theme ToTheme(CreateThemeRequest request);
    public static partial void Merge([MappingTarget] Theme theme, UpdateThemeRequest request);
    
    // EF Relations

    public static partial IQueryable<ThemeResponse> ProjectToResponse(this IQueryable<Theme> themes);
}