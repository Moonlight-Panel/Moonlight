using System.Net.Mail;
using Moonlight.App.Helpers;

namespace Moonlight.App.Services.Background;

public class TempMailService
{
    private string[] Domains = Array.Empty<string>();

    public TempMailService()
    {
        Task.Run(Init);
    }

    private async Task Init()
    {
        var client = new HttpClient();
        var text = await client.GetStringAsync("https://raw.githubusercontent.com/disposable-email-domains/disposable-email-domains/master/disposable_email_blocklist.conf");

        Domains = text
            .Split("\n")
            .Select(x => x.Trim())
            .ToArray();
        
        Logger.Info($"Fetched {Domains.Length} temp mail domains");
    }

    public Task<bool> IsTempMail(string mail)
    {
        var address = new MailAddress(mail);

        if (Domains.Contains(address.Host))
            return Task.FromResult(true);

        return Task.FromResult(false);
    }
}