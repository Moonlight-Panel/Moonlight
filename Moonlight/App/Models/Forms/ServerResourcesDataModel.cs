using System.ComponentModel.DataAnnotations;

namespace Moonlight.App.Models.Forms;

public class ServerResourcesDataModel
{
    [Required(ErrorMessage = "You need to specify the cpu cores")]
    public int Cpu { get; set; }
    
    [Required(ErrorMessage = "You need to specify the memory")]
    public long Memory { get; set; }
    
    [Required(ErrorMessage = "You need to specify the disk")]
    public long Disk { get; set; }
}