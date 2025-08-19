using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Models;

namespace Moonlight.ApiServer.Interfaces;

public interface IUserDeleteHandler
{
    public Task<UserDeleteValidationResult> Validate(User user);
    public Task Delete(User user, bool force);
}