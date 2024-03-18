using System.ComponentModel;

namespace Moonlight.Features.Servers.Actions;

public class ServerConfig
{
    [Description("The amount of cpu cores for a server instance. 100% = 1 Core")]
    public int Cpu { get; set; } = 100;

    [Description("The amount of memory in megabytes for a server instance")]
    public int Memory { get; set; } = 1024;

    [Description("The amount of disk space in megabytes for a server instance")]
    public int Disk { get; set; } = 1024;

    [Description("The id of the image to use for a server")]
    public int ImageId { get; set; } = 1;

    [Description(
        "The id of the node to use for the server. If not set, moonlight will search automaticly for the best node to deploy on")]
    public int NodeId { get; set; } = 0;

    [Description(
        "This options specifies if moonlight should give the server an allocation which ip has not been used by another server. So the server will has its own ip")]
    public bool DedicatedIp { get; set; } = false;
}