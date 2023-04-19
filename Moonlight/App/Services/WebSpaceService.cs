using Logging.Net;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.ApiClients.CloudPanel;
using Moonlight.App.ApiClients.CloudPanel.Requests;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Helpers;
using Moonlight.App.Helpers.Files;
using Moonlight.App.Models.Plesk.Requests;
using Moonlight.App.Models.Plesk.Resources;
using Moonlight.App.Repositories;
using FileAccess = Moonlight.App.Helpers.Files.FileAccess;

namespace Moonlight.App.Services;

public class WebSpaceService
{
    private readonly Repository<CloudPanel> CloudPanelRepository;
    private readonly Repository<WebSpace> WebSpaceRepository;
    private readonly CloudPanelApiHelper CloudPanelApiHelper;

    public WebSpaceService(Repository<CloudPanel> cloudPanelRepository, Repository<WebSpace> webSpaceRepository, CloudPanelApiHelper cloudPanelApiHelper)
    {
        CloudPanelRepository = cloudPanelRepository;
        WebSpaceRepository = webSpaceRepository;
        CloudPanelApiHelper = cloudPanelApiHelper;
    }

    public async Task<WebSpace> Create(string domain, User owner, CloudPanel? ps = null)
    {
        if (WebSpaceRepository.Get().Any(x => x.Domain == domain))
            throw new DisplayException("A website with this domain does already exist");

        var cloudPanel = ps ?? CloudPanelRepository.Get().First();

        var ftpLogin = domain.Replace(".", "_");
        var ftpPassword = StringHelper.GenerateString(16);

        var phpVersion = "8.1"; // TODO: Add config option or smth
        
        var w = new WebSpace()
        {
            CloudPanel = cloudPanel,
            Owner = owner,
            Domain = domain,
            UserName = ftpLogin,
            Password = ftpPassword,
            VHostTemplate = "Generic" //TODO: Implement as select option
        };

        var webSpace = WebSpaceRepository.Add(w);

        try
        {
            await CloudPanelApiHelper.Post(cloudPanel, "site/php", new AddPhpSite()
            {
                VHostTemplate = w.VHostTemplate,
                DomainName = w.Domain,
                PhpVersion = phpVersion,
                SiteUser = w.UserName,
                SiteUserPassword = w.Password
            });
        }
        catch (Exception)
        {
            WebSpaceRepository.Delete(webSpace);
            throw;
        }

        return webSpace;
    }

    public async Task Delete(WebSpace w)
    {
        var website = EnsureData(w);
        
        await CloudPanelApiHelper.Delete(website.CloudPanel, $"site/{website.Domain}", null);
        
        WebSpaceRepository.Delete(website);
    }

    public async Task<bool> IsHostUp(CloudPanel cloudPanel)
    {
        try
        {
            //var res = await PleskApiHelper.Get<ServerStatus>(pleskServer, "server");

            return true;

            //if (res != null)
            //    return true;
        }
        catch (Exception e)
        {
            // ignored
        }

        return false;
    }
    
    public async Task<bool> IsHostUp(WebSpace w)
    {
        var webSpace = EnsureData(w);

        return await IsHostUp(webSpace.CloudPanel);
    }

    #region SSL
    public async Task<string[]> GetSslCertificates(WebSpace w)
    {
        var certs = new List<string>();
        return certs.ToArray();
    }

    public async Task CreateSslCertificate(WebSpace w)
    {
        
    }

    public async Task DeleteSslCertificate(WebSpace w, string name)
    {
        
    }
    
    #endregion

    #region Databases

    public async Task<Models.Plesk.Resources.Database[]> GetDatabases(WebSpace w)
    {
        return Array.Empty<Models.Plesk.Resources.Database>();
    }

    public async Task CreateDatabase(WebSpace w, string name, string password)
    {
        
    }

    public async Task DeleteDatabase(WebSpace w, Models.Plesk.Resources.Database database)
    {
        
    }

    #endregion

    public Task<FileAccess> CreateFileAccess(WebSpace w)
    {
        var webspace = EnsureData(w);

        return Task.FromResult<FileAccess>(
            new SftpFileAccess(webspace.CloudPanel.Host, webspace.UserName, webspace.Password, 22, true)
        );
    }

    private WebSpace EnsureData(WebSpace webSpace)
    {
        if (webSpace.CloudPanel == null || webSpace.Owner == null)
            return WebSpaceRepository
                .Get()
                .Include(x => x.CloudPanel)
                .Include(x => x.Owner)
                .First(x => x.Id == webSpace.Id);
        
        return webSpace;
    }
}