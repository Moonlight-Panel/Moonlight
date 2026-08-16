using FluentResults;

namespace Moonlight.ApiSdk.Features.Users;

public interface IUserQueryService
{
    public Task<Result<User?>> FindByIdAsync(int id);
    public Task<Result<User?>> FindByUsernameAsync(string username);
    public Task<Result<User?>> FindByEmailAsync(string email);
    
    public Task<Result<IReadOnlyList<User>>> SearchByDisplayNameAsync(string displayName);
    public Task<Result<IReadOnlyList<User>>> SearchByEmailAsync(string email);
}