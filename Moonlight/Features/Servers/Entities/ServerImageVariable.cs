namespace Moonlight.Features.Servers.Entities;

public class ServerImageVariable
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string DefaultValue { get; set; }

    public string DisplayName { get; set; }
    public string Description { get; set; }
    public bool AllowUserToEdit { get; set; }
    public bool AllowUserToView { get; set; }
}