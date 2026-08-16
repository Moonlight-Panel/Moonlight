using System.ComponentModel.DataAnnotations;
using Moonlight.ApiSdk.Features.Roles;

namespace Moonlight.ApiSdk.Features.Users;

public class User
{
    public int Id { get; set; }

    // Base
    [MaxLength(64)]
    public string Username { get; set; }
    
    [MaxLength(32)]
    public string DisplayName { get; set; }
    
    [MaxLength(254)]
    public string? Email { get; set; }
    public bool IsDeleted { get; set; }
    
    // Local Auth
    public bool AllowLocalAuth { get; set; }
    
    [MaxLength(1024)]
    public string? PasswordHash { get; set; }
    
    // Roles
    public List<RoleMembership> Roles { get; set; } = [];
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}