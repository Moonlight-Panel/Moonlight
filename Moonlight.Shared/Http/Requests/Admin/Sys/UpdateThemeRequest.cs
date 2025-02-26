using System.ComponentModel.DataAnnotations;

namespace Moonlight.Shared.Http.Requests.Admin.Sys;

public class UpdateThemeRequest
{
    [Required(ErrorMessage = "You need to provide Variables")]
    public Dictionary<string, string> Variables { get; set; }
}