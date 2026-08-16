using FluentResults;

namespace Moonlight.ApiSdk.Features.Roles;

public interface IRoleQueryService
{
    public Task<Result<Role?>> FindByIdAsync(int roleId);
    public Task<Result<IReadOnlyList<Role>>> SearchByDisplayNameAsync(string displayName);
    
    public Task<Result<IReadOnlyList<Role>>> GetOfUserAsync(int userId);
    public Task<Result<IReadOnlyList<int>>> GetMembersAsync(int roleId);
    
    public Task<Result<string[]>> GetPermissionsAsync(int roleId);
}