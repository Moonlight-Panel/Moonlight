using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Moonlight.Core.Models.Forms.ApiKeys;

public class UpdateApiKeyForm
{
    [Required(ErrorMessage = "You need to provide a description")]
    [Description("Write a note here for which application the api key is used for")]
    public string Description { get; set; } = "";
    
    [Required(ErrorMessage = "You need to specify the expiration date of the api key")]
    [Description("Specify when the api key should expire")]
    public DateTime ExpiresAt { get; set; }

    [Required(ErrorMessage = "You need to specify what permissions the api key should have")]
    [DisplayName("Permissions")]
    public string PermissionJson { get; set; } = "[]";
}