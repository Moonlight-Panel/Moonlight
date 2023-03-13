using aaPanelSharp;
using Microsoft.EntityFrameworkCore;
using Moonlight.App.Database.Entities;
using Moonlight.App.Exceptions;
using Moonlight.App.Repositories;

namespace Moonlight.App.Services;

public class WebsiteService
{
    private readonly WebsiteRepository WebsiteRepository;

    public WebsiteService(WebsiteRepository websiteRepository)
    {
        WebsiteRepository = websiteRepository;
    }

    public Website Create(AaPanel aaPanel, User user, string name)
    {
        if (WebsiteRepository.Get().Any(x => x.DomainName == name))
            throw new DisplayException("A website with this domain has already been created");

        var access = new aaPanel(aaPanel.Url, aaPanel.Key);
        return null;
    }

    private aaPanel CreateApiAccess(Website website)
    {
        if (website.AaPanel == null)
        {
            website = WebsiteRepository
                .Get()
                .Include(x => x.AaPanel)
                .First(x => x.Id == website.Id);
        }
        
        return new aaPanel(website.AaPanel.Url, website.AaPanel.Key);
    }
}