namespace Moonlight.Shared.Http.Responses.Admin.ApiKeys;

public class ApiKeyResponse
{
    public int Id { get; set; }
    public string Description { get; set; }
    public string PermissionsJson { get; set; } = "[]";
    public DateTime ExpiresAt { get; set; }
}