using Microsoft.EntityFrameworkCore;
using Moonlight.App.ApiClients.CloudPanel;
using Moonlight.App.ApiClients.CloudPanel.Requests;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Helpers;
using Moonlight.App.Helpers.Files;
using Moonlight.App.Repositories;
using FileAccess = Moonlight.App.Helpers.Files.FileAccess;

namespace Moonlight.App.Services;

public class WebSpaceService
{
    private readonly Repository<CloudPanel> CloudPanelRepository;
    private readonly Repository<WebSpace> WebSpaceRepository;
    private readonly Repository<MySqlDatabase> DatabaseRepository;

    private readonly CloudPanelApiHelper CloudPanelApiHelper;

    public WebSpaceService(Repository<CloudPanel> cloudPanelRepository, Repository<WebSpace> webSpaceRepository, CloudPanelApiHelper cloudPanelApiHelper, Repository<MySqlDatabase> databaseRepository)
    {
        CloudPanelRepository = cloudPanelRepository;
        WebSpaceRepository = webSpaceRepository;
        CloudPanelApiHelper = cloudPanelApiHelper;
        DatabaseRepository = databaseRepository;
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
        var webSpace = WebSpaceRepository
            .Get()
            .Include(x => x.Databases)
            .Include(x => x.CloudPanel)
            .Include(x => x.Owner)
            .First(x => x.Id == w.Id);

        foreach (var database in webSpace.Databases.ToArray())
        {
            await DeleteDatabase(webSpace, database);
        }
        
        await CloudPanelApiHelper.Delete(webSpace.CloudPanel, $"site/{webSpace.Domain}", null);
        
        WebSpaceRepository.Delete(webSpace);
    }

    public async Task<bool> IsHostUp(CloudPanel cloudPanel)
    {
        try
        {
            await CloudPanelApiHelper.Post(cloudPanel, "", null);

            return true;
        }
        catch (CloudPanelException e)
        {
            if (e.StatusCode == 404)
                return true;
        }
        catch (Exception)
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

    public async Task IssueSslCertificate(WebSpace w)
    {
        var webspace = EnsureData(w);

        await CloudPanelApiHelper.Post(webspace.CloudPanel, "letsencrypt/install/certificate", new InstallLetsEncrypt()
        {
            DomainName = webspace.Domain
        });
    }

    #region Databases

    public Task<MySqlDatabase[]> GetDatabases(WebSpace w)
    {
        return Task.FromResult(WebSpaceRepository
            .Get()
            .Include(x => x.Databases)
            .First(x => x.Id == w.Id)
            .Databases.ToArray());
    }

    public async Task CreateDatabase(WebSpace w, string name, string password)
    {
        if (DatabaseRepository.Get().Any(x => x.UserName == name))
            throw new DisplayException("A database with this name does already exist");

        var webspace = EnsureData(w);

        var database = new MySqlDatabase()
        {
            UserName = name,
            Password = password
        };

        await CloudPanelApiHelper.Post(webspace.CloudPanel, "db", new AddDatabase()
        {
            DomainName = webspace.Domain,
            DatabaseName = database.UserName,
            DatabaseUserName = database.UserName,
            DatabaseUserPassword = database.Password
        });
        
        webspace.Databases.Add(database);
        WebSpaceRepository.Update(webspace);
    }

    public async Task DeleteDatabase(WebSpace w, MySqlDatabase database)
    {
        var webspace = EnsureData(w);
        
        await CloudPanelApiHelper.Delete(webspace.CloudPanel, $"db/{database.UserName}", null);

        webspace.Databases.Remove(database);
        WebSpaceRepository.Update(webspace);
    }

    #endregion

    public Task<FileAccess> CreateFileAccess(WebSpace w)
    {
        var webspace = EnsureData(w);

        return Task.FromResult<FileAccess>(
            new SftpFileAccess(webspace.CloudPanel.Host, webspace.UserName, webspace.Password, 22, true, $"/htdocs/{webspace.Domain}")
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