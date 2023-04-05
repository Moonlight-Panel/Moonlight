using Logging.Net;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Helpers;
using Moonlight.App.Helpers.Files;
using Moonlight.App.Models.Plesk.Requests;
using Moonlight.App.Models.Plesk.Resources;
using Moonlight.App.Repositories;
using FileAccess = Moonlight.App.Helpers.Files.FileAccess;

namespace Moonlight.App.Services;

public class WebsiteService
{
    private readonly WebsiteRepository WebsiteRepository;
    private readonly PleskServerRepository PleskServerRepository;
    private readonly PleskApiHelper PleskApiHelper;

    public WebsiteService(WebsiteRepository websiteRepository, PleskApiHelper pleskApiHelper, PleskServerRepository pleskServerRepository)
    {
        WebsiteRepository = websiteRepository;
        PleskApiHelper = pleskApiHelper;
        PleskServerRepository = pleskServerRepository;
    }

    public async Task<Website> Create(string baseDomain, User owner, PleskServer? ps = null)
    {
        if (WebsiteRepository.Get().Any(x => x.BaseDomain == baseDomain))
            throw new DisplayException("A website with this domain does already exist");

        var pleskServer = ps ?? PleskServerRepository.Get().First();

        var ftpLogin = baseDomain;
        var ftpPassword = StringHelper.GenerateString(16);
        
        var w = new Website()
        {
            PleskServer = pleskServer,
            Owner = owner,
            BaseDomain = baseDomain,
            PleskId = 0,
            FtpPassword = ftpPassword,
            FtpLogin = ftpLogin
        };

        var website = WebsiteRepository.Add(w);

        try
        {
            var id = await GetAdminAccount(pleskServer);

            var result = await PleskApiHelper.Post<CreateResult>(pleskServer, "domains", new CreateDomain()
            {
                Description = $"moonlight website {website.Id}",
                Name = baseDomain,
                HostingType = "virtual",
                Plan = new()
                {
                    Name = "Unlimited"
                },
                HostingSettings = new()
                {
                    FtpLogin = ftpLogin,
                    FtpPassword = ftpPassword
                },
                OwnerClient = new()
                {
                    Id = id
                }
            });

            website.PleskId = result.Id;
            
            WebsiteRepository.Update(website);
        }
        catch (Exception e)
        {
            WebsiteRepository.Delete(website);
            throw;
        }

        return website;
    }

    public async Task Delete(Website w)
    {
        var website = EnsureData(w);
        
        await PleskApiHelper.Delete(website.PleskServer, $"domains/{w.PleskId}", null);
        
        WebsiteRepository.Delete(website);
    }

    public async Task<bool> IsHostUp(PleskServer pleskServer)
    {
        try
        {
            var res = await PleskApiHelper.Get<ServerStatus>(pleskServer, "server");

            if (res != null)
                return true;
        }
        catch (Exception e)
        {
            // ignored
        }

        return false;
    }
    
    public async Task<bool> IsHostUp(Website w)
    {
        var website = EnsureData(w);
        
        try
        {
            var res = await PleskApiHelper.Get<ServerStatus>(website.PleskServer, "server");

            if (res != null)
                return true;
        }
        catch (Exception)
        {
            // ignored
        }

        return false;
    }
    
    public async Task<string> GetHost(PleskServer pleskServer)
    {
        return (await PleskApiHelper.Get<ServerStatus>(pleskServer, "server")).Hostname;
    }

    private async Task<int> GetAdminAccount(PleskServer pleskServer)
    {
        var users = await PleskApiHelper.Get<Client[]>(pleskServer, "clients");

        var user = users.FirstOrDefault(x => x.Type == "admin");

        if (user == null)
            throw new DisplayException("No admin account in plesk found");

        return user.Id;
    }

    public async Task<string[]> GetSslCertificates(Website w)
    {
        var website = EnsureData(w);
        var certs = new List<string>();
        
        Logger.Debug("1");

        var data = await ExecuteCli(website.PleskServer, "certificate", p =>
        {
            p.Add("-l");
            p.Add("-domain");
            p.Add(w.BaseDomain);
        });
        
        Logger.Debug("2");
        
        string[] lines = data.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string line in lines)
        {
            if (line.Contains("Lets Encrypt"))
            {
                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var part in parts)
                {
                    Logger.Debug(part);
                }
                
                if(parts.Length > 6)
                    certs.Add($"{parts[4]} {parts[5]} {parts[6]}");
            }
            else if (line.Contains("Listing of SSL/TLS certificates repository was successful"))
            {
                // This line indicates the end of the certificate listing, so we can stop parsing
                break;
            }
        }

        return certs.ToArray();
    }

    public async Task CreateSslCertificate()
    {
        
    }

    public async Task<FileAccess> CreateFileAccess(Website w)
    {
        var website = EnsureData(w);
        var host = await GetHost(website.PleskServer);

        return new FtpFileAccess(host, 21, website.FtpLogin, website.FtpPassword);
    }

    private async Task<string> ExecuteCli(
        PleskServer server,
        string cli, Action<List<string>>? parameters = null,
        Action<Dictionary<string, string>>? variables = null
    )
    {
        var p = new List<string>();
        var v = new Dictionary<string, string>();
        
        parameters?.Invoke(p);
        variables?.Invoke(v);

        var req = new CliCall()
        {
            Env = v,
            Params = p
        };

        var res = await PleskApiHelper.Post<CliResult>(server, $"cli/{cli}/call", req);

        return res.Stdout;
    }

    private Website EnsureData(Website website)
    {
        if (website.PleskServer == null)
            return WebsiteRepository
                .Get()
                .Include(x => x.PleskServer)
                .First(x => x.Id == website.Id);
        
        return website;
    }
}