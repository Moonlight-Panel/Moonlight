using System.ComponentModel.DataAnnotations;

namespace Moonlight.Shared.Http.Requests.Admin.ApiKeys;

public class CreateApiKeyRequest
{
    [Required(ErrorMessage = "You need to specify a description")]
    public string Description { get; set; }
    
    [Required(ErrorMessage = "You need to specify permissions for the api key")]
    public string PermissionsJson { get; set; } = "[]";
    
    [Required(ErrorMessage = "You need to specify an expire date")]
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(30);
}