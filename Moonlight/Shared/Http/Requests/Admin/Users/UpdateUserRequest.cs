using System.ComponentModel.DataAnnotations;

namespace Moonlight.Shared.Http.Requests.Admin.Users;

public class UpdateUserRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? PermissionsJson { get; set; }
}