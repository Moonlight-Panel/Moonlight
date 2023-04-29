using System.ComponentModel.DataAnnotations;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Models.Forms;

public class ServerEditDataModel
{
    [Required(ErrorMessage = "You need to enter a name")]
    [MaxLength(32, ErrorMessage = "The name cannot be longer that 32 characters")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "You need to specify a user")]
    public User Owner { get; set; }
    
    [Required(ErrorMessage = "You need to specify the cpu cores")]
    public int Cpu { get; set; }
    
    [Required(ErrorMessage = "You need to specify the memory")]
    public long Memory { get; set; }
    
    [Required(ErrorMessage = "You need to specify the disk")]
    public long Disk { get; set; }
    
    public string OverrideStartup { get; set; }
    
    public int DockerImageIndex { get; set; }
    
    public bool IsCleanupException { get; set; }
}