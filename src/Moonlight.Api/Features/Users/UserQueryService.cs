using FluentResults;
using Microsoft.EntityFrameworkCore;
using Moonlight.Api.Infrastructure.Database;
using Moonlight.ApiSdk.Features.Users;

namespace Moonlight.Api.Features.Users;

public class UserQueryService : IUserQueryService
{
    private readonly DataContext _dataContext;

    public UserQueryService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<Result<User?>> FindByIdAsync(int id)
    {
        var user = await _dataContext
            .Users
            .FirstOrDefaultAsync(u => u.Id == id);

        return Result.Ok(user);
    }

    public async Task<Result<User?>> FindByUsernameAsync(string username)
    {
        var user = await _dataContext
            .Users
            .FirstOrDefaultAsync(u => u.Username == username);

        return Result.Ok(user);
    }

    public async Task<Result<User?>> FindByEmailAsync(string email)
    {
        var user = await _dataContext
            .Users
            .FirstOrDefaultAsync(u => u.Email == email);

        return Result.Ok(user);
    }

    public async Task<Result<IReadOnlyList<User>>> SearchByDisplayNameAsync(string displayName)
    {
        var users = await _dataContext
            .Users
            .Where(u => EF.Functions.ILike(u.DisplayName, $"%{displayName}%"))
            .ToArrayAsync();

        return Result.Ok<IReadOnlyList<User>>(users);
    }

    public async Task<Result<IReadOnlyList<User>>> SearchByEmailAsync(string email)
    {
        var users = await _dataContext.Users
            .Where(u => u.Email != null && EF.Functions.ILike(u.Email, $"%{email}%"))
            .ToArrayAsync();

        return Result.Ok<IReadOnlyList<User>>(users);
    }
}