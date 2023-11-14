using Moonlight.App.Database.Entities;
using Moonlight.App.Database.Entities.Store;

namespace Moonlight.App.Plugins.Contexts;

public class ServiceManageContext
{
    public Service Service { get; set; }
    public User User { get; set; }
    public Product Product { get; set; }
}