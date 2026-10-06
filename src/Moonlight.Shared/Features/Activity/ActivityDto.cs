using System.Text.Json;

namespace Moonlight.Shared.Features.Activity;

public class ActivityDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string Type { get; set; }
    public Dictionary<string, JsonElement> Parameters { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}