using System.Net;
using Moonlight.App.Helpers;

namespace Moonlight.App.Services.Mail;

public class TrashMailDetectorService
{
    private string[] Domains;
    
    public TrashMailDetectorService()
    {
        Logger.Info("Fetching trash mail list from github repository");
        
        using var wc = new WebClient();
        
        var lines = wc
            .DownloadString("https://raw.githubusercontent.com/Endelon-Hosting/TrashMailDomainDetector/main/trashmail_domains.md")
            .Replace("\r\n", "\n")
            .Split(new [] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

        Domains = GetDomains(lines).ToArray();
    }
    
    private IEnumerable<string> GetDomains(string[] lines)
    {
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                if (line.Contains("."))
                {
                    var domain = line.Remove(0, line.IndexOf(".", StringComparison.Ordinal) + 1).Trim();
                    if (domain.Contains("."))
                    {
                        yield return domain;
                    }
                }
            }
        }
    }
    
    public bool IsTrashEmail(string mail)
    {
        return Domains.Contains(mail.Split('@')[1]);
    }
}