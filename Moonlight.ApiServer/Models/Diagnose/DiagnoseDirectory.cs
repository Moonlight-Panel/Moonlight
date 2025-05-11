namespace Moonlight.ApiServer.Models.Diagnose;

public class DiagnoseDirectory : DiagnoseEntry
{
    public List<DiagnoseEntry> Children { get; set; } = new();
    public override bool IsDirectory => true;
}