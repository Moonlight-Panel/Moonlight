using MoonCore.Extended.Abstractions;
using Moonlight.ApiServer.Database.Entities;
using Moonlight.ApiServer.Interfaces;
using Moonlight.ApiServer.Models;

namespace Moonlight.ApiServer.Services;

public class UserDeletionService
{
    private readonly IUserDeleteHandler[] Handlers;
    private readonly DatabaseRepository<User> UserRepository;

    public UserDeletionService(
        IEnumerable<IUserDeleteHandler> handlers,
        DatabaseRepository<User> userRepository
    )
    {
        UserRepository = userRepository;
        Handlers = handlers.ToArray();
    }

    public async Task<UserDeleteValidationResult> ValidateAsync(User user)
    {
        foreach (var handler in Handlers)
        {
            var result = await handler.ValidateAsync(user);

            if (!result.IsAllowed)
                return result;
        }

        return UserDeleteValidationResult.Allow();
    }

    public async Task DeleteAsync(User user, bool force)
    {
        foreach (var handler in Handlers)
            await handler.DeleteAsync(user, force);

        await UserRepository.RemoveAsync(user);
    }
}