using System.Diagnostics.CodeAnalysis;
using Moonlight.ApiSdk.Features.Users;
using Moonlight.Shared.Features.Users;
using Riok.Mapperly.Abstractions;

namespace Moonlight.Api.Features.Users;

[Mapper]
[SuppressMessage("Mapper", "RMG020:Source member is not mapped to any target member")]
[SuppressMessage("Mapper", "RMG012:Source member was not found for target member")]
public static partial class UserMapper
{
    public static partial IQueryable<UserDto> ProjectToAdminDto(this IQueryable<User> users);
    public static partial UserDto MapToAdminDto(User user);
    public static partial User MapToEntity(CreateUserDto dto);
    public static partial void Merge([MappingTarget] User user, UpdateUserDto request);
}