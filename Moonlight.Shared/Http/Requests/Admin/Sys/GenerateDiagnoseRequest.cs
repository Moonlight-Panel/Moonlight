using System.ComponentModel.DataAnnotations;

namespace Moonlight.Shared.Http.Requests.Admin.Sys;

public class GenerateDiagnoseRequest
{
    [Required(ErrorMessage = "You need to define providers")]
    public string[] Providers { get; set; } = [];
}