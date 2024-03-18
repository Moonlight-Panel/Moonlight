using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Moonlight.Features.Servers.Models.Forms.Admin.Nodes;

public class CreateNodeForm
{
    [Required(ErrorMessage = "You need to specify a name")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "You need to specify a fqdn")]
    [Description("This needs to be the ip or domain of the node")]
    public string Fqdn { get; set; } = "";
    
    [Description("This enables ssl for the http conenctions to the node. Only enable this if you have the cert installed on the node")]
    public bool Ssl { get; set; }

    [Description("This is the http(s) port used by the node to allow communication to the node from the panel")]
    public int HttpPort { get; set; } = 8080;
    
    [Description("This is the ftp port the panel and the users use to access their servers filesystem")]
    public int FtpPort { get; set; } = 2021;
}