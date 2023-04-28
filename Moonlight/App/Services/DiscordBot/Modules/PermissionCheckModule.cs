using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Repositories;
using Moonlight.App.Repositories.Servers;

namespace Moonlight.App.Services.DiscordBot.Modules;

public class PermissionCheckModule : BaseModule
{

    public PermissionCheckModule(DiscordSocketClient client, ConfigService configService, IServiceScope scope) : base(client, configService, scope)
        { }
    public override Task RegisterCommands()
        { return Task.CompletedTask; }

    public bool IsAdminByDiscordId(ulong discordId)
    {

        var usersRepo = Scope.ServiceProvider.GetRequiredService<UserRepository>();

        var user = usersRepo.Get().FirstOrDefault(x => x.DiscordId == discordId);

        if (user != null)
        {
            return user.Admin;
        }

        return false;
    }

    public bool HasViewPermissionByDiscordId(ulong discordId, int serverId)
    {
        var serversRepo = Scope.ServiceProvider.GetRequiredService<ServerRepository>();

        var server = serversRepo.Get().Include(x => x.Owner).FirstOrDefault(x => x.Id == serverId);

        if (server != null && server.Owner.DiscordId == discordId)
        {
            return true;
        }

        return false;
    }

}