using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoonCore.Exceptions;
using MoonCore.Extended.Abstractions;
using MoonCore.Extended.Helpers;
using MoonCore.Extended.PermFilter;
using MoonCore.Models;
using Moonlight.ApiServer.Database.Entities;
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
    [RequirePermission("admin.users.read")]
    public async Task<IPagedData<UserResponse>> Get(
        [FromQuery] int page,
        [FromQuery] [Range(1, 100)] int pageSize = 50
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
                Username = x.Username
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
    [RequirePermission("admin.users.read")]
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
            Username = user.Username
        };
    }

    [HttpPost]
    [RequirePermission("admin.users.create")]
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
            PermissionsJson = request.PermissionsJson
        };

        var finalUser = await UserRepository.Add(user);

        return new UserResponse()
        {
            Id = finalUser.Id,
            Email = finalUser.Email,
            Username = finalUser.Username
        };
    }

    [HttpPatch("{id}")]
    [RequirePermission("admin.users.update")]
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
            user.TokenValidTimestamp = DateTime.UtcNow; // This change will get applied by the crud helper
        }

        user.Email = request.Email;
        user.Username = request.Username;
        // TODO: Add permissions update here

        await UserRepository.Update(user);

        return new UserResponse()
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username
        };
    }

    [HttpDelete("{id}")]
    [RequirePermission("admin.users.delete")]
    public async Task Delete([FromRoute] int id)
    {
        var user = await UserRepository
            .Get()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (user == null)
            throw new HttpApiException("No user with that id found", 404);

        await UserRepository.Remove(user);
    }
}