using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MoonCore.Exceptions;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Models;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Services;
using Moonlight.ApiServer.Mappers;
using Moonlight.Shared.Http.Requests.Admin.Users;
using Moonlight.Shared.Http.Responses.Admin.Users;

namespace Moonlight.ApiServer.Http.Controllers.Admin.Users;

[ApiController]
[Route("api/admin/users")]
public class UsersController : Controller
{
    private readonly DatabaseRepository<User> UserRepository;

    public UsersController(DatabaseRepository<User> userRepository)
    {
        UserRepository = userRepository;
    }

    [HttpGet]
    [Authorize(Policy = "permissions:admin.users.get")]
    public async Task<ActionResult<ICountedData<UserResponse>>> Get(
        [FromQuery] int startIndex,
        [FromQuery] int count,
        [FromQuery] string? orderBy,
        [FromQuery] string? filter,
        [FromQuery] string orderByDir = "asc"
    )
    {
        if (count > 100)
            return Problem("You cannot fetch more items than 100 at a time", statusCode: 400);
        
        IQueryable<User> query = UserRepository.Get();

        query = orderBy switch
        {
            nameof(Database.Entities.User.Id) => orderByDir == "desc"
                ? query.OrderByDescending(x => x.Id)
                : query.OrderBy(x => x.Id),
                
            nameof(Database.Entities.User.Username) => orderByDir == "desc"
                ? query.OrderByDescending(x => x.Username)
                : query.OrderBy(x => x.Username),
            
            nameof(Database.Entities.User.Email) => orderByDir == "desc"
                ? query.OrderByDescending(x => x.Email)
                : query.OrderBy(x => x.Email),
            
            _ => query.OrderBy(x => x.Id)
        };

        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(x =>
                EF.Functions.ILike(x.Username, $"%{filter}%") ||
                EF.Functions.ILike(x.Email, $"%{filter}%")
            );
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(startIndex)
            .Take(count)
            .AsNoTracking()
            .ProjectToResponse()
            .ToArrayAsync();

        return new CountedData<UserResponse>()
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "permissions:admin.users.get")]
    public async Task<ActionResult<UserResponse>> GetSingle(int id)
    {
        var user = await UserRepository
            .Get()
            .ProjectToResponse()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
            return Problem("No user with that id found", statusCode: 404);

        return user;
    }

    [HttpPost]
    [Authorize(Policy = "permissions:admin.users.create")]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request)
    {
        // Reformat values
        request.Username = request.Username.ToLower().Trim();
        request.Email = request.Email.ToLower().Trim();

        // Check for users with the same values
        if (UserRepository.Get().Any(x => x.Username == request.Username))
            return Problem("A user with that username already exists", statusCode: 400);

        if (UserRepository.Get().Any(x => x.Email == request.Email))
            return Problem("A user with that email address already exists", statusCode: 400);

        var hashedPassword = HashHelper.Hash(request.Password);

        var user = new User()
        {
            Email = request.Email,
            Username = request.Username,
            Password = hashedPassword,
            Permissions = request.Permissions
        };

        var finalUser = await UserRepository.AddAsync(user);

        return UserMapper.ToResponse(finalUser);
    }

    [HttpPatch("{id}")]
    [Authorize(Policy = "permissions:admin.users.update")]
    public async Task<ActionResult<UserResponse>> Update([FromRoute] int id, [FromBody] UpdateUserRequest request)
    {
        var user = await UserRepository
            .Get()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
            return Problem("No user with that id found", statusCode: 404);

        // Reformat values
        request.Username = request.Username.ToLower().Trim();
        request.Email = request.Email.ToLower().Trim();

        // Check for users with the same values
        if (UserRepository.Get().Any(x => x.Username == request.Username && x.Id != user.Id))
            return Problem("Another user with that username already exists", statusCode: 400);

        if (UserRepository.Get().Any(x => x.Email == request.Email && x.Id != user.Id))
            return Problem("Another user with that email address already exists", statusCode: 400);

        // Perform hashing the password if required
        if (!string.IsNullOrEmpty(request.Password))
        {
            user.Password = HashHelper.Hash(request.Password);
            user.TokenValidTimestamp = DateTime.UtcNow; // Log out user after password change
        }

        if (request.Permissions.Any(x => !user.Permissions.Contains(x)))
        {
            user.Permissions = request.Permissions;
            user.TokenValidTimestamp = DateTime.UtcNow; // Log out user after permission change
        }

        user.Email = request.Email;
        user.Username = request.Username;

        await UserRepository.UpdateAsync(user);

        return UserMapper.ToResponse(user);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "permissions:admin.users.delete")]
    public async Task<ActionResult> Delete([FromRoute] int id, [FromQuery] bool force = false)
    {
        var user = await UserRepository
            .Get()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
            return Problem("No user with that id found", statusCode: 404);

        var deletionService = HttpContext.RequestServices.GetRequiredService<UserDeletionService>();

        if (!force)
        {
            var validationResult = await deletionService.Validate(user);

            if (!validationResult.IsAllowed)
                return Problem("Unable to delete user", statusCode: 400, title: validationResult.Reason);
        }

        await deletionService.Delete(user, force);
        return NoContent();
    }
}