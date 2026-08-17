using System.Diagnostics.CodeAnalysis;
using Moonlight.ApiSdk.Features.Users;
using Moonlight.Shared.Features.Users;
using Riok.Mapperly.Abstractions;

namespace Moonlight.Api.Features.Users;

[Mapper]
[SuppressMessage("Mapper", "RMG020:Source member is not mapped to any target member")]
public static partial class UserMapper
{
    public static partial IQueryable<UserDto> ProjectToAdminDto(this IQueryable<User> users);
}