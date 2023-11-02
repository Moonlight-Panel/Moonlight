using Moonlight.App.Database.Entities;
using Moonlight.App.Models.Enums;
using Moonlight.App.Repositories;
using Moonlight.App.Services;
using Moonlight.App.Services.Users;

namespace Moonlight.App.Api.Requests.Auth;

[ApiRequest(5)]
public class IsEmailVerifiedRequest : AbstractRequest
{
    public bool SendMail { get; set; }
    public bool MailVerified { get; set; }
    
    public override async Task ProcessRequest()
    {
        if(Context.User == null)
            return;
        
        var userRepository = ServiceProvider.GetRequiredService<Repository<User>>();
        Context.User = userRepository.Get().Where(x => x.Id == Context.User.Id).ToArray()[0];
        
        if (SendMail && Context.User != null)
        {
            var userAuthService = ServiceProvider.GetRequiredService<UserAuthService>();
            await userAuthService.SendVerification(Context!.User);
        }

        var configService = ServiceProvider.GetRequiredService<ConfigService>();
        var reqEmailVerify = configService.Get().Security.EnableEmailVerify;
        if (Context?.User?.Flags!.Contains(UserFlag.MailVerified.ToString()) ?? false)
            MailVerified = true;
        else
            MailVerified = !reqEmailVerify;

        await Task.Delay(200);
    }

    public override ResponseDataBuilder CreateResponse(ResponseDataBuilder builder)
    {
        builder.WriteBoolean(MailVerified);

        return builder;
    }

    public override void ReadData(RequestDataContext dataContext)
    {
        SendMail = dataContext.ReadBoolean();
    }
}