using System.ComponentModel.DataAnnotations;

namespace Moonlight.Shared.Http.Requests.Admin.ApiKeys;

public class UpdateApiKeyRequest
{
    [Required(ErrorMessage = "You need to specify a description")]
    public string Description { get; set; }
}