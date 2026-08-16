using FluentResults;
using Microsoft.EntityFrameworkCore;
using Moonlight.Api.Infrastructure.Database;
using Moonlight.ApiSdk.Features.Roles;

namespace Moonlight.Api.Features.Roles;

public class RoleQueryService : IRoleQueryService
{
    private readonly DataContext _dataContext;

    public RoleQueryService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<Result<Role?>> FindByIdAsync(int roleId)
    {
        var role = await _dataContext
            .Roles
            .FirstOrDefaultAsync(r => r.Id == roleId);

        return Result.Ok(role);
    }

    public async Task<Result<IReadOnlyList<Role>>> SearchByDisplayNameAsync(string displayName)
    {
        var roles = await _dataContext
            .Roles
            .Where(r => EF.Functions.ILike(r.DisplayName, $"%{displayName}%"))
            .ToArrayAsync();

        return Result.Ok<IReadOnlyList<Role>>(roles);
    }

    public async Task<Result<IReadOnlyList<Role>>> GetOfUserAsync(int userId)
    {
        var roles = await _dataContext
            .RoleMemberships
            .Where(r => r.User.Id == userId)
            .Select(x => x.Role)
            .ToArrayAsync();

        return Result.Ok<IReadOnlyList<Role>>(roles);
    }

    public async Task<Result<IReadOnlyList<int>>> GetMembersAsync(int roleId)
    {
        var memberIds = await _dataContext
            .RoleMemberships
            .Where(m => m.Role.Id == roleId)
            .Select(m => m.User.Id)
            .ToArrayAsync();

        return Result.Ok<IReadOnlyList<int>>(memberIds);
    }

    public async Task<Result<string[]>> GetPermissionsAsync(int roleId)
    {
        var role = await _dataContext
            .Roles
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null)
            return Result.Fail($"Role with id {roleId} not found");

        return Result.Ok(role.Permissions);
    }
}