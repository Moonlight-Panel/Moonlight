using Moonlight.ApiSdk.Features.Users;

namespace Moonlight.ApiSdk.Features.Roles;

public class RoleMembership
{
    public int Id { get; set; }

    public User User { get; set; }
    public Role Role { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}