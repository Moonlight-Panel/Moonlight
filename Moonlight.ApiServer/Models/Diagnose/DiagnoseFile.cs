namespace Moonlight.ApiServer.Models.Diagnose;

public class DiagnoseFile : DiagnoseEntry
{
    public Func<byte[]> GetContent = () => [];
    
    public override bool IsDirectory => false;
}