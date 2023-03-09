using System.Net;
using System.Net.Mail;
using Logging.Net;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;

namespace Moonlight.App.Services;

public class MailService
{
    private readonly string Server;
    private readonly string Password;
    private readonly string Email;
    private readonly int Port;

    public MailService(ConfigService configService)
    {
        var mailConfig = configService
            .GetSection("Moonlight")
            .GetSection("Mail");

        Server = mailConfig.GetValue<string>("Server");
        Password = mailConfig.GetValue<string>("Password");
        Email = mailConfig.GetValue<string>("Email");
        Port = mailConfig.GetValue<int>("Port");
    }
    
    public async Task SendMail(
        User user,
        string name,
        Action<Dictionary<string, string>> values
    )
    {
        if (!File.Exists($"resources/mail/{name}.html"))
        {
            Logger.Warn($"Mail template '{name}' not found. Make sure to place one in the resources folder");
            throw new DisplayException("Mail template not found");
        }

        var rawHtml = await File.ReadAllTextAsync($"resources/mail/{name}.html");

        var val = new Dictionary<string, string>();
        values.Invoke(val);
        
        val.Add("FirstName", user.FirstName);
        val.Add("LastName", user.LastName);

        var parsed = ParseMail(rawHtml, val);

        Task.Run(async () =>
        {
            try
            {
                using var client = new SmtpClient();
        
                client.Host = Server;
                client.Port = Port;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(Email, Password);

                await client.SendMailAsync(new MailMessage()
                {
                    From = new MailAddress(Email),
                    Sender = new MailAddress(Email),
                    Body = parsed,
                    IsBodyHtml = true,
                    Subject = $"Hey {user.FirstName}, there are news from moonlight",
                    To = { new MailAddress(user.Email) }
                });
                
                Logger.Debug("Send!");
            }
            catch (Exception e)
            {
                Logger.Warn("Error sending mail");
                Logger.Warn(e);
            }
        });
    }

    private string ParseMail(string html, Dictionary<string, string> values)
    {
        foreach (var kvp in values)
        {
            html = html.Replace("{{" + kvp.Key + "}}", kvp.Value);
        }

        return html;
    }
}