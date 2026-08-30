using System.Diagnostics.CodeAnalysis;
using Moonlight.ApiSdk.Features.Roles;
using Moonlight.ApiSdk.Features.Users;
using Moonlight.Shared.Features.Roles;
using Riok.Mapperly.Abstractions;

namespace Moonlight.Api.Features.Roles;

[Mapper]
[SuppressMessage("Mapper", "RMG020:Source member is not mapped to any target member")]
[SuppressMessage("Mapper", "RMG012:Source member was not found for target member")]
public static partial class RoleMapper
{
    public static IQueryable<RoleDto> ProjectToAdminDto(this IQueryable<Role> roles)
    {
        return roles.Select(role => new RoleDto()
        {
            Id = role.Id,
            DisplayName = role.DisplayName,
            Description = role.Description,
            Permissions = role.Permissions,
            UpdatedAt = role.UpdatedAt,
            CreatedAt = role.CreatedAt,
            MemberCount = role.Members.Count
        });
    }

    [MapProperty("Members.Count", "MemberCount")]
    public static partial RoleDto MapToAdminDto(Role role);

    public static partial Role MapToEntity(CreateRoleDto dto);
    public static partial void Merge([MappingTarget] Role role, UpdateRoleDto request);

    public static IQueryable<MembershipDto> ProjectToAdminDto(
        this IQueryable<RoleMembership> roleMemberships
    )
    {
        return roleMemberships.Select(roleMembership => new MembershipDto()
        {
            Id = roleMembership.Id,
            CreatedAt = roleMembership.CreatedAt,
            DisplayName = roleMembership.User.DisplayName,
            UserId = roleMembership.User.Id
        });
    }
}