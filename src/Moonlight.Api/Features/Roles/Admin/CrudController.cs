using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moonlight.Api.Infrastructure.Database;
using Moonlight.ApiSdk.Features.Misc;
using Moonlight.ApiSdk.Features.Roles;
using Moonlight.Shared.Features.Roles;

namespace Moonlight.Api.Features.Roles.Admin;

[Authorize]
[ApiController]
[Route("api/admin/roles")]
public class CrudController : Controller
{
    private readonly DataContext _dataContext;

    public CrudController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    [HttpGet]
    public async Task<ActionResult<RangedData<RoleDto>>> GetAsync(
        [FromQuery] int limit,
        [FromQuery] int offset,
        [FromQuery] string? search = null
    )
    {
        if (limit > 100)
            return Problem("You cannot retrieve more than 100 items", statusCode: 400);

        IQueryable<Role> query = _dataContext
            .Roles
            .OrderBy(x => x.Id);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r =>
                EF.Functions.ILike(r.DisplayName, $"%{search}%") ||
                EF.Functions.ILike(r.Description, $"%{search}%")
            );
        }

        var totalLength = await query.CountAsync();

        var roles = await query
            .Skip(offset)
            .Take(limit)
            .ProjectToAdminDto()
            .ToArrayAsync();

        return new RangedData<RoleDto>(roles, totalLength);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoleDto>> GetAsync([FromRoute] int id)
    {
        var roleDto = await _dataContext
            .Roles
            .Where(x => x.Id == id)
            .ProjectToAdminDto()
            .FirstOrDefaultAsync();

        if (roleDto is null)
            return Problem("Item with this id not found", statusCode: 404);

        return roleDto;
    }

    [HttpPost]
    public async Task<ActionResult<RoleDto>> CreateAsync([FromBody] CreateRoleDto request)
    {
        var role = RoleMapper.MapToEntity(request);

        role.UpdatedAt = DateTimeOffset.UtcNow;
        role.CreatedAt = DateTimeOffset.UtcNow;

        var addedRole = _dataContext.Roles.Add(role);
        await _dataContext.SaveChangesAsync();

        return RoleMapper.MapToAdminDto(addedRole.Entity);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RoleDto>> UpdateAsync([FromRoute] int id, [FromBody] UpdateRoleDto request)
    {
        var role = await _dataContext
            .Roles
            .Include(r => r.Members)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (role is null)
            return Problem("No role with that id found", statusCode: 404);

        RoleMapper.Merge(role, request);

        role.UpdatedAt = DateTimeOffset.UtcNow;

        _dataContext.Roles.Update(role);
        await _dataContext.SaveChangesAsync();

        return RoleMapper.MapToAdminDto(role);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int id)
    {
        var role = await _dataContext
            .Roles
            .FirstOrDefaultAsync(x => x.Id == id);

        if (role is null)
            return Problem("No role with that id found", statusCode: 404);

        _dataContext.Roles.Remove(role);
        await _dataContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id:int}/members")]
    public async Task<ActionResult<RangedData<MembershipDto>>> GetMembersAsync(
        [FromRoute] int id,
        [FromQuery] int offset,
        [FromQuery] int limit
    )
    {
        if (limit > 100)
            return Problem("You cannot retrieve more than 100 items", statusCode: 400);
        
        var roleExists = await _dataContext.Roles.AnyAsync(r => r.Id == id);

        if (!roleExists)
            return Problem("No role with that id found", statusCode: 404);

        var query = _dataContext
            .RoleMemberships
            .Where(x => x.Role.Id == id && !x.User.IsDeleted);

        var count = await query.CountAsync();

        var members = await query
            .ProjectToAdminDto()
            .OrderBy(u => u.Id)
            .Skip(offset)
            .Take(limit)
            .ToArrayAsync();

        return new RangedData<MembershipDto>(members, count);
    }

    [HttpPost("{id:int}/members")]
    public async Task<ActionResult> AddMemberAsync([FromRoute] int id, [FromBody] AddMembershipDto request)
    {
        var role = await _dataContext.Roles.FirstOrDefaultAsync(r => r.Id == id);

        if (role is null)
            return Problem("No role with that id found", statusCode: 404);

        var user = await _dataContext
            .Users
            .Where(u => !u.IsDeleted)
            .FirstOrDefaultAsync(u => u.Id == request.UserId);

        if (user is null)
            return Problem("No user with that id found", statusCode: 404);

        var alreadyMember = await _dataContext
            .RoleMemberships
            .AnyAsync(m => m.Role.Id == id && m.User.Id == request.UserId);

        if (alreadyMember)
            return Problem("User is already a member of that role", statusCode: 400);

        _dataContext.RoleMemberships.Add(new RoleMembership
        {
            User = user,
            Role = role,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dataContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}/members/{userId:int}")]
    public async Task<ActionResult> RemoveMemberAsync([FromRoute] int id, [FromRoute] int userId)
    {
        var membership = await _dataContext
            .RoleMemberships
            .FirstOrDefaultAsync(m => m.Role.Id == id && m.User.Id == userId);

        if (membership is null)
            return Problem("User is not a member of that role", statusCode: 404);

        _dataContext.RoleMemberships.Remove(membership);
        await _dataContext.SaveChangesAsync();

        return NoContent();
    }
}