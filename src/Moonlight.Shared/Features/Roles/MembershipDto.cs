namespace Moonlight.Shared.Features.Roles;

public class MembershipDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string DisplayName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}