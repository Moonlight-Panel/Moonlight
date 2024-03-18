using MoonCore.Abstractions;
using MoonCore.Exceptions;
using MoonCore.Helpers;
using Moonlight.Core.Database.Entities;
using Moonlight.Core.Models.Abstractions;
using OtpNet;

namespace Moonlight.Core.Services;

public class DefaultAuthenticationProvider : IAuthenticationProvider
{
    private readonly Repository<User> UserRepository;

    public DefaultAuthenticationProvider(Repository<User> userRepository)
    {
        UserRepository = userRepository;
    }

    public Task<bool> RequiresTwoFactorCode(string email, string password)
    {
        var user = VerifyEmailAndPassword(email, password);

        if (user == null)
            return Task.FromResult(false);

        return Task.FromResult(user.Totp);
    }

    public Task<User?> Authenticate(string email, string password, string twoFactorCode)
    {
        var user = VerifyEmailAndPassword(email, password);

        if (user == null)
            return Task.FromResult<User?>(null);

        // Check if totp is enabled, if not, we are done here
        if (!user.Totp)
            return Task.FromResult<User?>(user);

        // Validate two factor input
        if (string.IsNullOrEmpty(twoFactorCode))
            throw new DisplayException("Please enter a valid two factor code");

        if (string.IsNullOrEmpty(user.TotpSecret))
            throw new DisplayException("Totp secret is missing. Please contact the administrator to resolve the issue");

        // Calculate server side 2fa code
        var totp = new Totp(Base32Encoding.ToBytes(user.TotpSecret));
        var serverSide2Fa = totp.ComputeTotp();

        // Validate two factor code
        if (twoFactorCode != serverSide2Fa)
            throw new DisplayException("Invalid two factor code");

        return Task.FromResult<User?>(user);
    }

    public Task<User?> Register(string username, string email, string password)
    {
        username = username
            .Trim()
            .ToLower();

        if (UserRepository.Get().Any(x => x.Username == username))
            throw new DisplayException("A user with this username does already exist");

        if (GetUserByEmail(email) != null)
            throw new DisplayException("A user with this email does already exist");

        var user = new User()
        {
            Email = email,
            Username = username,
            Password = HashHelper.HashToString(password)
        };

        var finishedUser = UserRepository.Add(user);

        return Task.FromResult<User?>(finishedUser);
    }

    public Task ChangePassword(User u, string password)
    {
        // Ensure we have a fresh instance from the data context
        var user = UserRepository
            .Get()
            .First(x => x.Id == u.Id);

        // Update the password and save the changes
        user.Password = HashHelper.HashToString(password);
        user.TokenValidTimestamp = DateTime.UtcNow;
        UserRepository.Update(user);

        return Task.CompletedTask;
    }

    public Task ChangeDetails(User user, string newEmail, string newUsername)
    {
        if (UserRepository.Get().Any(x => x.Username == newUsername && x.Id != user.Id))
            throw new DisplayException("A user with this username does already exist");

        var userWithThatEmail = GetUserByEmail(newEmail);

        if (userWithThatEmail != null && userWithThatEmail.Id != user.Id)
            throw new DisplayException("A user with this email does already exist");

        user.Email = newEmail.Trim().ToLower();
        user.Username = newUsername;
        
        UserRepository.Update(user);
        
        return Task.CompletedTask;
    }

    public Task SetTwoFactorSecret(User user, string secret)
    {
        user.TotpSecret = secret;
        user.Totp = !string.IsNullOrEmpty(secret);
        UserRepository.Update(user);

        return Task.CompletedTask;
    }

    private User? VerifyEmailAndPassword(string email, string password)
    {
        var user = GetUserByEmail(email);

        if (user == null) // Unknown email
            return null;

        // Verify password
        if (!HashHelper.Verify(password, user.Password))
            return null;

        return user;
    }

    private User? GetUserByEmail(string email)
    {
        email = email
            .Trim()
            .ToLower();

        var user = UserRepository
            .Get()
            .FirstOrDefault(x => x.Email == email);

        return user;
    }
}