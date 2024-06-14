using MoonCore.Abstractions;
using Moonlight.Features.Servers.Entities;
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

    public static async Task FixVariables(Server server, Repository<Server> repository)
    {
        
        foreach (var imageVariable in server.Image.Variables)
        {
            
            
            // if the variable is not in the servers variables, add it
            if (!server.Variables.Any(x => x.Key == imageVariable.Key))
            {
                
                server.Variables.Add(new ServerVariable()
                {
                    Key = imageVariable.Key,
                    Value = imageVariable.DefaultValue
                });
                Console.WriteLine("Added variable "+ imageVariable.Key);
            }
        }
            
        // remove unused variables
        HashSet<string> newKeys = new(server.Image.Variables.Select(x => x.Key));
        server.Variables.RemoveAll(x => !newKeys.Contains(x.Key));

        repository.Update(server);
    }
}