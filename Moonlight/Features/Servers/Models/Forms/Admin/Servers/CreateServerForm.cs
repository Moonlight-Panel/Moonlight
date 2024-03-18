using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MoonCoreUI.Attributes;
using Moonlight.Core.Database.Entities;
using Moonlight.Features.Servers.Entities;

namespace Moonlight.Features.Servers.Models.Forms.Admin.Servers;

public class CreateServerForm
{
    [Required(ErrorMessage = "You need to specify a name")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "You need to specify a server owner")]
    [Selector(SelectorProp = "Username", DisplayProp = "Username")]
    public User Owner { get; set; }
    
    [Required(ErrorMessage = "You need to specify a server image")]
    [Selector(SelectorProp = "Name", DisplayProp = "Name")]
    public ServerImage Image { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Enter a valid cpu value")]
    [Description("The cores the server will be able to use. 100 = 1 Core")]
    public int Cpu { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Enter a valid memory value")]
    [Description("The amount of memory this server will be able to use in megabytes")]
    public int Memory { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Enter a valid disk value")]
    [Description("The amount of disk space this server will be able to use in megabytes")]
    public int Disk { get; set; }
    
    [Description("Whether to use a virtual disk for storing server files. Dont use this if you want to overallocate as the virtual disks will fill out the space you allocate")]
    public bool UseVirtualDisk { get; set; }
    
    [Required(ErrorMessage = "You need to specify a server node")]
    [Selector(SelectorProp = "Name", DisplayProp = "Name")]
    public ServerNode Node { get; set; }
}