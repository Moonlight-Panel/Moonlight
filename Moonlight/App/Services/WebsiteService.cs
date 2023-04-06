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
    private readonly UserRepository UserRepository;

    public WebsiteService(WebsiteRepository websiteRepository, PleskApiHelper pleskApiHelper, PleskServerRepository pleskServerRepository, UserRepository userRepository)
    {
        WebsiteRepository = websiteRepository;
        PleskApiHelper = pleskApiHelper;
        PleskServerRepository = pleskServerRepository;
        UserRepository = userRepository;
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
    
    #region Get host
    
    public async Task<string> GetHost(PleskServer pleskServer)
    {
        return (await PleskApiHelper.Get<ServerStatus>(pleskServer, "server")).Hostname;
    }
    
    public async Task<string> GetHost(Website w)
    {
        var website = EnsureData(w);

        return await GetHost(website.PleskServer);
    }
    
    #endregion

    private async Task<int> GetAdminAccount(PleskServer pleskServer)
    {
        var users = await PleskApiHelper.Get<Client[]>(pleskServer, "clients");

        var user = users.FirstOrDefault(x => x.Type == "admin");

        if (user == null)
            throw new DisplayException("No admin account in plesk found");

        return user.Id;
    }

    #region SSL
    public async Task<string[]> GetSslCertificates(Website w)
    {
        var website = EnsureData(w);
        var certs = new List<string>();

        var data = await ExecuteCli(website.PleskServer, "certificate", p =>
        {
            p.Add("-l");
            p.Add("-domain");
            p.Add(w.BaseDomain);
        });

        string[] lines = data.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string line in lines)
        {
            if (line.Contains("Lets Encrypt"))
            {
                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

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

    public async Task CreateSslCertificate(Website w)
    {
        var website = EnsureData(w);

        await ExecuteCli(website.PleskServer, "extension", p =>
        {
            p.Add("--exec");
            p.Add("letsencrypt");
            p.Add("cli.php");
            p.Add("-d");
            p.Add(website.BaseDomain);
            p.Add("-m");
            p.Add(website.Owner.Email);
        });
    }

    public async Task DeleteSslCertificate(Website w, string name)
    {
        var website = EnsureData(w);

        try
        {
            await ExecuteCli(website.PleskServer, "site", p =>
            {
                p.Add("-u");
                p.Add(website.BaseDomain);
                p.Add("-ssl");
                p.Add("false");
            });

            try
            {
                await ExecuteCli(website.PleskServer, "certificate", p =>
                {
                    p.Add("--remove");
                    p.Add(name);
                    p.Add("-domain");
                    p.Add(website.BaseDomain);
                });
            }
            catch (Exception e)
            {
                Logger.Warn("Error removing ssl certificate");
                Logger.Warn(e);

                throw new DisplayException("An unknown error occured while removing ssl certificate");
            }
        }
        catch (DisplayException)
        {
            // Redirect all display exception to soft error handler
            throw;
        }
        catch (Exception e)
        {
            Logger.Warn("Error disabling ssl certificate");
            Logger.Warn(e);

            throw new DisplayException("An unknown error occured while disabling ssl certificate");
        }
    }
    
    #endregion

    #region Databases

    public async Task<Models.Plesk.Resources.Database[]> GetDatabases(Website w)
    {
        var website = EnsureData(w);

        var dbs = await PleskApiHelper.Get<Models.Plesk.Resources.Database[]>(
            website.PleskServer, 
            $"databases?domain={w.BaseDomain}"
        );

        return dbs;
    }

    public async Task CreateDatabase(Website w, string name, string password)
    {
        var website = EnsureData(w);

        var server = await GetDefaultDatabaseServer(website);

        if (server == null)
            throw new DisplayException("No database server marked as default found");

        var dbReq = new CreateDatabase()
        {
            Name = name,
            Type = "mysql",
            ParentDomain = new()
            {
                Name = website.BaseDomain
            },
            ServerId = server.Id
        };

        var db = await PleskApiHelper.Post<Models.Plesk.Resources.Database>(website.PleskServer, "databases", dbReq);

        if (db == null)
            throw new DisplayException("Unable to create database via api");

        var dbUserReq = new CreateDatabaseUser()
        {
            DatabaseId = db.Id,
            Login = name,
            Password = password
        };

        await PleskApiHelper.Post(website.PleskServer, "dbusers", dbUserReq);
    }

    public async Task DeleteDatabase(Website w, Models.Plesk.Resources.Database database)
    {
        var website = EnsureData(w);

        var dbUsers = await PleskApiHelper.Get<DatabaseUser[]>(
            website.PleskServer,
            $"dbusers?dbId={database.Id}"
        );

        foreach (var dbUser in dbUsers)
        {
            await PleskApiHelper.Delete(website.PleskServer, $"dbusers/{dbUser.Id}", null);
        }

        await PleskApiHelper.Delete(website.PleskServer, $"databases/{database.Id}", null);
    }

    public async Task<DatabaseServer?> GetDefaultDatabaseServer(PleskServer pleskServer)
    {
        var dbServers = await PleskApiHelper.Get<DatabaseServer[]>(pleskServer, "dbservers");

        return dbServers.FirstOrDefault(x => x.IsDefault);
    }

    public async Task<DatabaseServer?> GetDefaultDatabaseServer(Website w)
    {
        var website = EnsureData(w);

        return await GetDefaultDatabaseServer(website.PleskServer);
    }

    #endregion

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
        if (website.PleskServer == null || website.Owner == null)
            return WebsiteRepository
                .Get()
                .Include(x => x.PleskServer)
                .Include(x => x.Owner)
                .First(x => x.Id == website.Id);
        
        return website;
    }
}