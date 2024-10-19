using System.ComponentModel.DataAnnotations;

namespace Moonlight.Shared.Http.Requests.Auth;

public class RefreshRequest
{
    [Required(ErrorMessage = "You need to provide a refresh token")]
    public string RefreshToken { get; set; }
}