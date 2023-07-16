namespace Moonlight.App.Database.Entities;

public class PermissionGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public byte[] Permissions { get; set; } = Array.Empty<byte>();
}