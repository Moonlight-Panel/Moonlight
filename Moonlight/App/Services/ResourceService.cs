using Moonlight.App.Database.Entities;

namespace Moonlight.App.Services;

public class ResourceService
{
    private readonly string AppUrl;
    
    public ResourceService(ConfigService configService)
    {
        AppUrl = configService.GetSection("Moonlight").GetValue<string>("AppUrl");
    }

    public string Image(string name)
    {
        return $"{AppUrl}/api/moonlight/resources/images/{name}";
    }

    public string Avatar(User user)
    {
        return $"{AppUrl}/api/moonlight/avatar/{user.Id}";
    }
}