using Moonlight.App.Database.Entities;
using Moonlight.App.Repositories;
using Moonlight.App.Services.Utils;

namespace Moonlight.App.Api.Requests.Auth;

[ApiRequest(2)]
public class TokenBasedLoginRequest: AbstractRequest
{
    private String Token { get; set; }
    private bool Success { get; set; } = false;
    public override ResponseDataBuilder CreateResponse(ResponseDataBuilder builder)
    {
        builder.WriteBoolean(Success);
        
        return builder;
    }

    public override async Task ProcessRequest()
    {
        var jwtService = ServiceProvider.GetRequiredService<JwtService>();
        var userRepository = ServiceProvider.GetRequiredService<Repository<User>>();
        if (string.IsNullOrEmpty(Token))
            return;

        if (!await jwtService.Validate(Token))
            return;

        var data = await jwtService.Decode(Token);

        if (!data.ContainsKey("userId"))
            return;

        var userId = int.Parse(data["userId"]);

        var user = userRepository
            .Get()
            .FirstOrDefault(x => x.Id == userId);

        if (user == null)
            return;

        if (!data.ContainsKey("issuedAt"))
            return;

        var issuedAt = long.Parse(data["issuedAt"]);
        var issuedAtDateTime = DateTimeOffset.FromUnixTimeSeconds(issuedAt).DateTime;

        // If the valid time is newer then when the token was issued, the token is not longer valid
        if (user.TokenValidTimestamp > issuedAtDateTime)
            return;

        Context!.User = user;

        if (Context.User == null) // If the current user is null, stop loading additional data
            return;

        Success = true;
    }

    public override void ReadData(RequestDataContext dataContext)
    {
        Token = dataContext.ReadString();
    }
}