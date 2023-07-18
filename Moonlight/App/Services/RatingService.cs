using Moonlight.App.Database.Entities;
using Moonlight.App.Events;
using Moonlight.App.Repositories;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Services;

public class RatingService
{
    private readonly IdentityService IdentityService;
    private readonly EventSystem Event;
    private readonly Repository<User> UserRepository;

    private readonly bool Enabled = false;
    private readonly string Url = "";
    private readonly int MinRating = 4;
    private readonly int DaysSince = 5;

    public RatingService(
        IdentityService identityService,
        ConfigService configService,
        EventSystem eventSystem,
        Repository<User> userRepository)
    {
        IdentityService = identityService;
        Event = eventSystem;
        UserRepository = userRepository;

        var config = configService.Get().Moonlight.Rating;

        Enabled = config.Enabled;
        Url = config.Url;
        MinRating = config.MinRating;
        DaysSince = config.DaysSince;
    }

    public async Task<bool> ShouldRate()
    {
        if (!Enabled)
            return false;

        var user = IdentityService.User;

        if (user == null)
            return false;

        if (user.HasRated)
            return false;

        if ((DateTime.UtcNow - user.CreatedAt).TotalDays >= DaysSince)
        {
            return true;
        }

        return false;
    }

    public Task<string> GetRateUrl()
    {
        return Task.FromResult(Url);
    }

    public async Task<bool> Rate(int rate)
    {
        var user = IdentityService.User;
        
        // Double check states:
        
        if(user == null)
            return false;
        
        if(user.HasRated)
            return false;

        user.HasRated = true;
        user.Rating = rate;
        
        UserRepository.Update(user);
        await Event.Emit("user.rating", user);

        if (rate >= MinRating)
            return true;

        return false;
    }
}