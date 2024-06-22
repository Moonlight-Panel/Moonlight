using System.ComponentModel.DataAnnotations;
using Moonlight.Features.Servers.Entities;

namespace Moonlight.Features.Servers.Models.Forms.Users.Networks;

public class CreateNetworkForm
{
    [Required(ErrorMessage = "You need to specify a name for the network")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "You need to specify a node to create the network on")]
    //[Selector(SelectorProp = "Name", DisplayProp = "Name")]
    public ServerNode Node { get; set; }
}