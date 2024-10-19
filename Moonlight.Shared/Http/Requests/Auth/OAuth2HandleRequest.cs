using System.ComponentModel.DataAnnotations;

namespace Moonlight.Shared.Http.Requests.Auth;

public class OAuth2HandleRequest
{
    [Required(ErrorMessage = "You need to provide the oauth2 code")]
    public string Code { get; set; }
}