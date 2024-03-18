using System.ComponentModel.DataAnnotations;

namespace Moonlight.Features.Servers.Models.Forms.Admin.Nodes;

public class UpdateAllocationForm
{
    [Required(ErrorMessage = "You need to provide a bind ip address")]
    public string IpAddress { get; set; } = "0.0.0.0";
    
    [Range(1, 65535, ErrorMessage = "You need to provide a valid port")]
    public int Port { get; set; }
}