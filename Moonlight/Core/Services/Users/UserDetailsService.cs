using MoonCore.Abstractions;
using MoonCore.Attributes;
using Moonlight.Core.Database.Entities;


namespace Moonlight.Core.Services.Users;

[Scoped]
public class UserDetailsService
{
    private readonly BucketService BucketService;
    private readonly Repository<User> UserRepository;

    public UserDetailsService(BucketService bucketService, Repository<User> userRepository)
    {
        BucketService = bucketService;
        UserRepository = userRepository;
    }

    public async Task UpdateAvatar(User user, Stream stream, string fileName)
    {
        if (user.Avatar != null)
        {
            await BucketService.Delete("avatars", user.Avatar, true);
        }
        
        var file = await BucketService.Store("avatars", stream, fileName);

        user.Avatar = file;
        UserRepository.Update(user);
    }

    public async Task UpdateAvatar(User user) // Overload to reset avatar
    {
        if (user.Avatar != null)
        {
            await BucketService.Delete("avatars", user.Avatar, true);
        }
        
        user.Avatar = null;
        UserRepository.Update(user);
    }
}