namespace Moonlight.Shared.Shared;

public class ProblemDetailsDto
{
    public string Type { get; set; }
    public string Title { get; set; }
    public int Status { get; set; }
    public string? Detail { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}