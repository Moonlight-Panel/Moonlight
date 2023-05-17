using System.ComponentModel.DataAnnotations;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Models.Forms;

public class ServerDataModel
{
    [Required(ErrorMessage = "You need to enter a name")]
    [MaxLength(32, ErrorMessage = "The name cannot be longer that 32 characters")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "You need to specify a owner")]
    public User Owner { get; set; }

    [Required(ErrorMessage = "You need to specify cpu amount")]
    public int Cpu { get; set; } = 100;

    [Required(ErrorMessage = "You need to specify a memory amount")]
    public int Memory { get; set; } = 1024;
    
    [Required(ErrorMessage = "You need to specify a disk amount")]
    public int Disk { get; set; } = 1024;
    
    [Required(ErrorMessage = "You need to specify a image")]
    public Image Image { get; set; }
    
    [Required(ErrorMessage = "You need to specify a node")]
    public Node Node { get; set; }

    public string OverrideStartup { get; set; } = "";
    
    public int DockerImageIndex { get; set; }
}