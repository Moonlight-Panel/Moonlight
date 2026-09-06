using System.ComponentModel.DataAnnotations.Schema;
using Moonlight.ApiSdk.Features.Users;

namespace Moonlight.ApiSdk.Features.Activity;

public class Activity
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string Type { get; set; }
    [Column(TypeName = "jsonb")] public string Parameters { get; set; }
    public User? User { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}