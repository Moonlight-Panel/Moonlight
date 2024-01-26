using Moonlight.Core.Database.Entities;

namespace Moonlight.Features.ServiceManagement.Entities;

public class ServiceShare
{
    public int Id { get; set; }
    public User User { get; set; }
}