using Moonlight.App.Helpers.Files;

namespace Moonlight.App.Models.Misc;

public class MailTemplate // This is just for the blazor table at /admin/system/mail
{
    public string Name { get; set; } = "";
    public FileData File { get; set; }
}