using Moonlight.Features.Servers.Models.Enums;

namespace Moonlight.Features.Servers.Helpers;

public static class ServerUtilsHelper
{
    public static string GetColorFromState(ServerState state)
    {
        var color = "secondary";

        switch (state)
        {
            case ServerState.Stopping:
                color = "warning";
                break;

            case ServerState.Starting:
                color = "warning";
                break;

            case ServerState.Offline:
                color = "danger";
                break;

            case ServerState.Online:
                color = "success";
                break;

            case ServerState.Installing:
                color = "primary";
                break;

            case ServerState.Join2Start:
                color = "info";
                break;
        }

        return color;
    }
}