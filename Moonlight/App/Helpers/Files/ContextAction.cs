namespace Moonlight.App.Helpers.Files;

public class ContextAction
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public Action<FileData> Action { get; set; }
}