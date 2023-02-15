namespace Moonlight.App.Database.Entities;

public class ServerBackup
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Guid Uuid { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Created { get; set; } = false;
    public long Bytes { get; set; }
}