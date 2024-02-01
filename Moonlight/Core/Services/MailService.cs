using MailKit.Net.Smtp;
using MimeKit;
using MoonCore.Attributes;
using MoonCore.Helpers;
using MoonCore.Services;
using Moonlight.Core.Configuration;
using Moonlight.Core.Database.Entities;

namespace Moonlight.Core.Services;

[Singleton]
public class MailService
{
    private readonly ConfigService<ConfigV1> ConfigService;
    private readonly string BasePath;

    public MailService(ConfigService<ConfigV1> configService)
    {
        ConfigService = configService;

        BasePath = PathBuilder.Dir("storage", "mail");
        Directory.CreateDirectory(BasePath);
    }

    public async Task Send(User user, string title, string templateName, params object[] models)
    {
        var config = ConfigService.Get().MailServer;

        try
        {
            // Build mail message
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                config.SenderName,
                config.Email
            ));

            message.To.Add(new MailboxAddress(
                $"{user.Username}",
                user.Email
            ));

            message.Subject = Formatter.ProcessTemplating(title, models);

            var body = new BodyBuilder()
            {
                HtmlBody = await ParseTemplate(templateName, models)
            };
            message.Body = body.ToMessageBody();

            // The actual sending will not be done in the mail thread to prevent long loading times
            Task.Run(async () =>
            {
                using var smtpClient = new SmtpClient();
                
                try
                {
                    await smtpClient.ConnectAsync(config.Host, config.Port, config.UseSsl);
                    await smtpClient.AuthenticateAsync(config.Email, config.Password);
                    await smtpClient.SendAsync(message);
                    await smtpClient.DisconnectAsync(true);
                }
                catch (Exception e)
                {
                    Logger.Warn("An unexpected error occured while connecting and transferring mail to mailserver");
                    Logger.Warn(e);
                }
            });
        }
        catch (FileNotFoundException)
        {
            // ignored as we log it anyways in the parse template function
        }
        catch (Exception e)
        {
            Logger.Warn("Unhandled error occured during sending mail:");
            Logger.Warn(e);
        }
    }

    private async Task<string> ParseTemplate(string templateName, params object[] models)
    {
        if (!File.Exists(PathBuilder.File(BasePath, templateName + ".html")))
        {
            Logger.Warn($"Mail template '{templateName}' is missing. Skipping sending mail");
            throw new FileNotFoundException();
        }

        var text = await File.ReadAllTextAsync(
            PathBuilder.File(BasePath, templateName + ".html")
        );

        // For details how the templating works, check out the explanation of the ProcessTemplating in the Formatter class
        text = Formatter.ProcessTemplating(text, models);

        return text;
    }

    // Helpers

    public async Task Send(IEnumerable<User> users, string title, string templateName, params object[] models)
    {
        foreach (var user in users)
            await Send(user, title, templateName, models);
    }
}