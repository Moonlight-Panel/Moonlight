using System.ComponentModel.DataAnnotations;

namespace Moonlight.Features.Servers.Models.Forms.Users.Networks;

public class UpdateNetworkForm
{
    [Required(ErrorMessage = "You need to specify a name for the network")]
    public string Name { get; set; }
}