using System.ComponentModel.DataAnnotations;

namespace Moonlight.ApiSdk.Features.Roles;

public class Role
{
    public int Id { get; set; }

    [MaxLength(32)]
    public string DisplayName { get; set; }
    
    [MaxLength(512)]
    public string Description { get; set; }
    public string[] Permissions { get; set; }

    public List<RoleMembership> Members { get; set; } = [];
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}