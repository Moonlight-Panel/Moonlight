using MoonCore.Abstractions;
using MoonCore.Attributes;
using MoonCore.Exceptions;
using Moonlight.Core.Database.Entities;

namespace Moonlight.Core.Services.Users;

[Scoped]
public class UserService
{
    private readonly Repository<User> UserRepository;
    private readonly IServiceProvider ServiceProvider;

    public UserAuthService Auth => ServiceProvider.GetRequiredService<UserAuthService>();
    public UserDetailsService Details => ServiceProvider.GetRequiredService<UserDetailsService>();
    public UserDeleteService Delete => ServiceProvider.GetRequiredService<UserDeleteService>();
    
    public UserService(
        Repository<User> userRepository,
        IServiceProvider serviceProvider)
    {
        UserRepository = userRepository;
        ServiceProvider = serviceProvider;
    }

    public Task Update(User user, string username, string email)
    {
        // Event though we have form validation i want to
        // ensure that at least these basic formatting things are done
        email = email.ToLower().Trim();
        username = username.ToLower().Trim();

        // Prevent duplication or username and/or email
        if (UserRepository.Get().Any(x => x.Email == email && x.Id != user.Id))
            throw new DisplayException("A user with that email does already exist");

        if (UserRepository.Get().Any(x => x.Username == username && x.Id != user.Id))
            throw new DisplayException("A user with that username does already exist");

        user.Username = username;
        user.Email = email;

        UserRepository.Update(user);

        return Task.CompletedTask;
    }
}