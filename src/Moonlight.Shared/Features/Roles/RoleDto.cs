namespace Moonlight.Shared.Features.Roles;

public class RoleDto
{
    public int Id { get; set; }
    
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string[] Permissions { get; set; }
    
    public int MemberCount { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
