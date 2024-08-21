using System.ComponentModel.DataAnnotations;

namespace Moonlight.Shared.Http.Requests.Admin.ApiKeys;

public class CreateApiKeyRequest
{
    [Required(ErrorMessage = "You need to provide a name for the api key")]
    public string Name { get; set; }
    
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "You need to provide the permissions the api key should have")]
    public string PermissionsJson { get; set; }
    
    [Required(ErrorMessage = "You need to provide an expire date")]
    public DateTime ExpireDate { get; set; }
}