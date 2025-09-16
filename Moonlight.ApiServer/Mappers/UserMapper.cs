using Moonlight.ApiServer.Database.Entities;
using Moonlight.Shared.Http.Responses.Admin.Users;
using Riok.Mapperly.Abstractions;

namespace Moonlight.ApiServer.Mappers;

[Mapper]
public static partial class UserMapper
{
    // Mappers
    public static partial UserResponse ToResponse(User user);
    
    // EF Relations
    public static partial IQueryable<UserResponse> ProjectToResponse(this IQueryable<User> users);
}