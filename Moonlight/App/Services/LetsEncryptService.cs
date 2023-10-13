using System.Security.Cryptography.X509Certificates;
using Certes;
using Certes.Acme;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Connections;
using Moonlight.App.Events;
using Moonlight.App.Helpers;

namespace Moonlight.App.Services;

public class LetsEncryptService
{
    private readonly ConfigService ConfigService;
    private readonly string LetsEncryptCertPath;
    private readonly EventSystem Event;
    private X509Certificate2 Certificate;

    public string HttpChallenge { get; private set; } = "";
    public string HttpChallengeToken { get; private set; } = "";

    public LetsEncryptService(ConfigService configService, EventSystem eventSystem)
    {
        ConfigService = configService;
        Event = eventSystem;
        LetsEncryptCertPath = PathBuilder.File("storage", "certs", "letsencrypt.pfx");
    }

    public async Task AutoProcess()
    {
       try
       {
             if (!ConfigService.Get().Moonlight.LetsEncrypt.Enable)
            return;

            if (await CheckNeedsRenewal())
            {
                try
                {
                    await Renew();
                }
                catch (Exception e)
                {
                    Logger.Error("Unable to issue lets encrypt certificate");
                    Logger.Error(e);
                }
            }
            else
                Logger.Info("Skipping lets encrypt renewal");

            await LoadCertificate();
       }
       catch(Exception e)
       {
            Logger.Error("Unhandled exception while auto proccessing lets encrypt certificates");
            Logger.Error(e);
       }
    }

    private Task LoadCertificate()
    {
        try
        {
            Certificate = new X509Certificate2(
                LetsEncryptCertPath, 
                ConfigService.Get().Moonlight.Security.Token
            );
            
            Logger.Info($"Loaded ssl certificate. '{Certificate.FriendlyName}' issued by '{Certificate.IssuerName.Name}'");
        }
        catch (Exception e)
        {
            Logger.Warn("Unable to load ssl certificates");
            Logger.Warn(e);
        }
        
        return Task.CompletedTask;
    }
    
    private async Task Renew()
    {
        Logger.Info("Renewing lets encrypt certificate");
        
        var uri = new Uri(ConfigService.Get().Moonlight.AppUrl);
        var config = ConfigService.Get().Moonlight.LetsEncrypt;

        if (uri.HostNameType == UriHostNameType.IPv4 || uri.HostNameType == UriHostNameType.IPv6)
        {
            Logger.Warn("You cannot use an ip to issue a lets encrypt certificate");
            return;
        }

        var acmeContext = new AcmeContext(WellKnownServers.LetsEncryptV2);

        Logger.Info($"Starting lets encrypt certificate issuing. Using acme server '{acmeContext.DirectoryUri}'");

        var account = await acmeContext.NewAccount(config.ExpireEmail, true);

        Logger.Info("Creating order");
        var order = await acmeContext.NewOrder(new[] { uri.Host });
        var authZ = (await order.Authorizations()).First();

        var challenge = await authZ.Http();

        HttpChallengeToken = challenge.Token;
        HttpChallenge = challenge.KeyAuthz;

        Logger.Info("Waiting for http challenge to complete");

        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(3));

            try
            {
                await challenge.Validate();
                Logger.Info("Tried to validate");
            }
            catch (Exception e)
            {
                Logger.Error("Unable to validate challenge");
                Logger.Error(e);
            }
        });

        await Event.WaitForEvent<Object>("letsEncrypt.challengeFetched", this);

        Logger.Info("Generating certificate");

        var privateKey = KeyFactory.NewKey(KeyAlgorithm.ES256);

        var certificate = await order.Generate(new CsrInfo
        {
            CountryName = config.CountryCode,
            State = config.State,
            Locality = config.Locality,
            Organization = config.Organization,
            OrganizationUnit = "Dev",
            CommonName = uri.Host
        }, privateKey);

        var builder = certificate.ToPfx(privateKey);

        var certBytes = builder.Build(
            uri.Host,
            ConfigService.Get().Moonlight.Security.Token
        );

        Logger.Info($"Saved lets encrypt certificate to '{LetsEncryptCertPath}'");
        await File.WriteAllBytesAsync(LetsEncryptCertPath, certBytes);
    }

    private Task<bool> CheckNeedsRenewal()
    {
        if (!File.Exists(LetsEncryptCertPath))
        {
            Logger.Info("No lets encrypt certificate found");
            return Task.FromResult(true);
        }

        var existingCert = new X509Certificate2(LetsEncryptCertPath, ConfigService.Get().Moonlight.Security.Token);
        var expirationDate = existingCert.NotAfter;

        if (DateTime.Now < expirationDate)
        {
            Logger.Info($"Lets encrypt certificate valid until {Formatter.FormatDate(expirationDate)}");
            return Task.FromResult(false);
        }

        Logger.Info("Lets encrypt certificate expired");
        return Task.FromResult(true);
    }

    public X509Certificate2? SelectCertificate(ConnectionContext? context, string? domain)
    {
        if (context == null)
            return null;

        return Certificate;
    }
}