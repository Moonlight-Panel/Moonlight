namespace Moonlight.Shared.Http.Responses.Admin.ApiKeys;

public class CreateApiKeyResponse
{
    public int Id { get; set; }
    public string Secret { get; set; }
    public string Description { get; set; }
    public string[] Permissions { get; set; } = [];
    public DateTimeOffset ExpiresAt { get; set; }
}