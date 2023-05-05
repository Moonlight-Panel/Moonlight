namespace Moonlight.App.ApiClients.Daemon.Resources;

public class ContainerStats
{
    public List<Container> Containers { get; set; } = new();
    
    public class Container
    {
        public string Name { get; set; }
        public long Memory { get; set; }
        public double Cpu { get; set; }
        public long NetworkIn { get; set; }
        public long NetworkOut { get; set; }
    }
}