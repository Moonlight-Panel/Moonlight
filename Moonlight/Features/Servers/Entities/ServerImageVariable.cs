using Moonlight.Features.Servers.Entities.Enums;

namespace Moonlight.Features.Servers.Entities;

public class ServerImageVariable
{
    public int Id { get; set; }

    public string Key { get; set; } = "";
    public string DefaultValue { get; set; } = "";

    public string DisplayName { get; set; } = "";
    public string Description { get; set; } = "";

    public bool AllowView { get; set; } = false;
    public bool AllowEdit { get; set; } = false;

    public string? Filter { get; set; }
    public ServerImageVariableType Type { get; set; } = ServerImageVariableType.Text;
}