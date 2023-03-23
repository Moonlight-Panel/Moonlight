using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Helpers;
using Moonlight.App.Models.Plesk.Requests;
using Moonlight.App.Models.Plesk.Resources;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services;

public class WebsiteService
{
    private readonly WebsiteRepository WebsiteRepository;
    private readonly PleskServerRepository PleskServerRepository;
    private readonly PleskApiHelper PleskApiHelper;

    public WebsiteService(WebsiteRepository websiteRepository, PleskServerRepository pleskServerRepository,
        PleskApiHelper pleskApiHelper)
    {
        WebsiteRepository = websiteRepository;
        PleskServerRepository = pleskServerRepository;
        PleskApiHelper = pleskApiHelper;
    }

    public async Task<Website> Create(User owner, string baseDomain, string ftpPassword, PleskServer? ps = null)
    {
        var pleskServer = ps ?? PleskServerRepository.Get().FirstOrDefault();

        if (pleskServer == null)
            throw new DisplayException("No plesk server found to deploy the website");
        
        var website = WebsiteRepository.Add(new Website()
        {
            PleskServer = pleskServer,
            Owner = owner,
            BaseDomain = baseDomain
        });

        try
        {
            var userId = await GetAdminAccountForDeploy(pleskServer);

            string ftpUsername = baseDomain.Replace(".", "_");

            var createDomain = new CreateDomain()
            {
                Description = "Moonlight website " + website.Id,
                Name = baseDomain,
                OwnerClient = new()
                {
                    Id = userId
                },
                Plan = new()
                {
                    Name = "Unlimited"
                },
                HostingType = "virtual",
                HostingSettings = new()
                {
                    FtpLogin = ftpUsername,
                    FtpPassword = ftpPassword
                }
            };

            var identifier = await PleskApiHelper.Post<Identifier>(
                pleskServer, 
                "domains",
                createDomain
            );

            website.PleskId = identifier.Id;
            
            WebsiteRepository.Update(website);

            return website;
        }
        catch (Exception e)
        {
            WebsiteRepository.Delete(website);

            throw;
        }
    }

    public async Task Delete(Website w)
    {
        var website = WebsiteRepository
            .Get()
            .Include(x => x.PleskServer)
            .FirstOrDefault(x => x.Id == w.Id);

        if (website == null)
            throw new DisplayException("Website not found");

        await PleskApiHelper.Delete(website.PleskServer, $"domains/{website.PleskId}", null);
        
        WebsiteRepository.Delete(website);
    }

    private async Task<int> GetAdminAccountForDeploy(PleskServer pleskServer)
    {
        var clients = await PleskApiHelper.Get<Client[]>(pleskServer, "clients");
        var adminClient = clients.FirstOrDefault(x => x.Type == "admin");

        if (adminClient == null)
            throw new DisplayException("Unable to deploy website. Plesk admin account is missing");

        return adminClient.Id;
    }
}