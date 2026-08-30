using System.ComponentModel.DataAnnotations;

namespace Moonlight.Shared.Features.Roles;

public class UpdateRoleDto
{
    [Required]
    [MaxLength(32)]
    public string DisplayName { get; set; }
    
    [Required]
    [MaxLength(512)]
    public string Description { get; set; }
    
    public string[] Permissions { get; set; }
}
