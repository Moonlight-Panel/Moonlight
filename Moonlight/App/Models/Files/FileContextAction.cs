namespace Moonlight.App.Models.Files;

public class FileContextAction
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Action<FileManagerObject> Action { get; set; }
}