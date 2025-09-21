using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Models;

namespace Moonlight.ApiServer.Interfaces;

public interface IUserDeleteHandler
{
    public Task<UserDeleteValidationResult> ValidateAsync(User user);
    public Task DeleteAsync(User user, bool force);
}