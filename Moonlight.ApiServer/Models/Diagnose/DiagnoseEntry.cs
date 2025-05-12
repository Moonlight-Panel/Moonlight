namespace Moonlight.ApiServer.Models.Diagnose;

public abstract class DiagnoseEntry
{
    public required string Name { get; set; } = "";
    
    public abstract bool IsDirectory { get; }
}