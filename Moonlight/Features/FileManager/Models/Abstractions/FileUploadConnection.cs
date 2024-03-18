namespace Moonlight.Features.FileManager.Models.Abstractions;

public class FileUploadConnection
{
    public int Id { get; set; }
    public string Url { get; set; }
    public Func<FileUpload, Task>? OnFileReceived { get; set; }
    public Func<string, Task>? OnUrlChanged { get; set; }
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
}