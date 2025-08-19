using System.ComponentModel.DataAnnotations;
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
    public async Task<IPagedData<UserResponse>> Get(
        [FromQuery] [Range(0, int.MaxValue)] int page,
        [FromQuery] [Range(1, 100)] int pageSize
    )
    {
        var count = await UserRepository.Get().CountAsync();

        var users = await UserRepository
            .Get()
            .OrderBy(x => x.Id)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToArrayAsync();

        var mappedUsers = users
            .Select(x => new UserResponse()
            {
                Id = x.Id,
                Email = x.Email,
                Username = x.Username,
                Permissions = x.Permissions
            })
            .ToArray();

        return new PagedData<UserResponse>()
        {
            CurrentPage = page,
            Items = mappedUsers,
            PageSize = pageSize,
            TotalItems = count,
            TotalPages = count == 0 ? 0 : (count - 1) / pageSize
        };
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "permissions:admin.users.get")]
    public async Task<UserResponse> GetSingle(int id)
    {
        var user = await UserRepository
            .Get()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
            throw new HttpApiException("No user with that id found", 404);

        return new UserResponse()
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Permissions = user.Permissions
        };
    }

    [HttpPost]
    [Authorize(Policy = "permissions:admin.users.create")]
    public async Task<UserResponse> Create([FromBody] CreateUserRequest request)
    {
        // Reformat values
        request.Username = request.Username.ToLower().Trim();
        request.Email = request.Email.ToLower().Trim();

        // Check for users with the same values
        if (UserRepository.Get().Any(x => x.Username == request.Username))
            throw new HttpApiException("A user with that username already exists", 400);

        if (UserRepository.Get().Any(x => x.Email == request.Email))
            throw new HttpApiException("A user with that email address already exists", 400);

        var hashedPassword = HashHelper.Hash(request.Password);

        var user = new User()
        {
            Email = request.Email,
            Username = request.Username,
            Password = hashedPassword,
            Permissions = request.Permissions
        };

        var finalUser = await UserRepository.Add(user);

        return new UserResponse()
        {
            Id = finalUser.Id,
            Email = finalUser.Email,
            Username = finalUser.Username,
            Permissions = finalUser.Permissions
        };
    }

    [HttpPatch("{id}")]
    [Authorize(Policy = "permissions:admin.users.update")]
    public async Task<UserResponse> Update([FromRoute] int id, [FromBody] UpdateUserRequest request)
    {
        var user = await UserRepository
            .Get()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
            throw new HttpApiException("No user with that id found", 404);

        // Reformat values
        request.Username = request.Username.ToLower().Trim();
        request.Email = request.Email.ToLower().Trim();

        // Check for users with the same values
        if (UserRepository.Get().Any(x => x.Username == request.Username && x.Id != user.Id))
            throw new HttpApiException("A user with that username already exists", 400);

        if (UserRepository.Get().Any(x => x.Email == request.Email && x.Id != user.Id))
            throw new HttpApiException("A user with that email address already exists", 400);

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

        await UserRepository.Update(user);

        return new UserResponse()
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Permissions = user.Permissions
        };
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "permissions:admin.users.delete")]
    public async Task Delete([FromRoute] int id, [FromQuery] bool force = false)
    {
        var user = await UserRepository
            .Get()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
            throw new HttpApiException("No user with that id found", 404);

        var deletionService = HttpContext.RequestServices.GetRequiredService<UserDeletionService>();

        if (!force)
        {
            var validationResult = await deletionService.Validate(user);

            if (!validationResult.IsAllowed)
                throw new HttpApiException($"Unable to delete user", 400, validationResult.Reason);
        }

        await deletionService.Delete(user, force);
    }
}