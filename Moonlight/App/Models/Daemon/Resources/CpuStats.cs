namespace Moonlight.App.Models.Daemon.Resources;

public class CpuStats
{
    public double Usage { get; set; }
    public int Cores { get; set; }
    public string Model { get; set; } = "";
}