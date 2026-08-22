using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moonlight.Api.Infrastructure.Database;
using Moonlight.ApiSdk.Features.Misc;
using Moonlight.ApiSdk.Features.Users;
using Moonlight.Shared.Features.Users;

namespace Moonlight.Api.Features.Users.Admin;

[Authorize]
[ApiController]
[Route("api/admin/users")]
public class CrudController : Controller
{
    private readonly DataContext _dataContext;

    public CrudController(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    [HttpGet]
    public async Task<ActionResult<RangedData<UserDto>>> GetAsync(
        [FromQuery] int limit,
        [FromQuery] int offset,
        [FromQuery] string? search = null
    )
    {
        if (limit > 100)
            return Problem("You cannot retrieve more than 100 items", statusCode: 400);

        IQueryable<User> query = _dataContext
            .Users
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Id);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u =>
                EF.Functions.ILike(u.DisplayName, $"%{search}%") ||
                EF.Functions.ILike(u.Email, $"%{search}%") ||
                EF.Functions.ILike(u.Username, $"%{search}%")
            );
        }

        var totalLength = await query.CountAsync();

        var users = await query
            .Skip(offset)
            .Take(limit)
            .ProjectToAdminDto()
            .ToArrayAsync();

        return new RangedData<UserDto>(users, totalLength);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateAsync([FromBody] CreateUserDto request)
    {
        if (request.AllowLocalAuth && string.IsNullOrWhiteSpace(request.Password))
            return Problem("Password is required when enabling local auth", statusCode: 400);

        var usernameFound = await _dataContext
            .Users
            .AnyAsync(x => x.Username == request.Username);

        if (usernameFound)
            return Problem("Username already exists", statusCode: 400);

        var user = UserMapper.MapToEntity(request);

        user.UpdatedAt = DateTimeOffset.UtcNow;
        user.CreatedAt = DateTimeOffset.UtcNow;

        if (user.AllowLocalAuth)
            user.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);

        var addedUser = _dataContext.Users.Add(user);
        await _dataContext.SaveChangesAsync();

        return UserMapper.MapToAdminDto(addedUser.Entity);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> UpdateAsync([FromRoute] int id, [FromBody] UpdateUserDto request)
    {
        var user = await _dataContext
            .Users
            .Where(x => !x.IsDeleted)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user is null)
            return Problem("No user with that id found", statusCode: 404);

        var usernameExists = _dataContext
            .Users
            .Any(x => x.Username == request.Username && x.Id != user.Id);

        if (usernameExists)
            return Problem("A user with that username already exists", statusCode: 400);

        // Local Auth enable
        if (!user.AllowLocalAuth && request.AllowLocalAuth)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
                return Problem("In order to enable local authentication you need to provide a password");

            user.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);
        }

        // Password Update
        if (user.AllowLocalAuth && request.AllowLocalAuth && !string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);
        
        UserMapper.Merge(user, request);

        _dataContext.Users.Update(user);
        await _dataContext.SaveChangesAsync();
        
        return UserMapper.MapToAdminDto(user);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteAsync([FromRoute] int id)
    {
        var user = await _dataContext
            .Users
            .Where(x => !x.IsDeleted)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user is null)
            return Problem("No user with that id found", statusCode: 404);

        user.IsDeleted = true;

        _dataContext.Users.Update(user);
        await _dataContext.SaveChangesAsync();
        
        return NoContent();
    }
}