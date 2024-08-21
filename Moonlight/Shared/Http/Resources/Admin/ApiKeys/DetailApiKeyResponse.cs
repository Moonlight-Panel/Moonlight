namespace Moonlight.Shared.Http.Resources.Admin.ApiKeys;

public class DetailApiKeyResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; } = "";
    public string PermissionsJson { get; set; }
    public DateTime ExpireDate { get; set; }
}