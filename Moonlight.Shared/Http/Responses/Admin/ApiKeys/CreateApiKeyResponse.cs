namespace Moonlight.Shared.Http.Responses.Admin.ApiKeys;

public class CreateApiKeyResponse
{
    public int Id { get; set; }
    public string Secret { get; set; }
    public string Description { get; set; }
    public string PermissionsJson { get; set; } = "[]";
    public DateTime ExpiresAt { get; set; }
}