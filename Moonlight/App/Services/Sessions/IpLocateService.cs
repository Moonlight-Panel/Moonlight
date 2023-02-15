using System.Net;
using Moonlight.App.Models.IpLocate.Resources;
using Newtonsoft.Json;

namespace Moonlight.App.Services.Sessions;

public class IpLocateService
{
    private readonly IdentityService IdentityService;

    public IpLocateService(IdentityService identityService)
    {
        IdentityService = identityService;
    }

    public async Task<string> GetLocation()
    {
        var ip = IdentityService.GetIp();
        var location = "N/A";

        if (ip != "N/A")
        {
            using (var wc = new WebClient())
            {
                var res = JsonConvert.DeserializeObject<IpLocate>(
                    await wc.DownloadStringTaskAsync(
                        $"http://ip-api.com/json/{ip}"
                        )
                );

                location = $"{res.Country} - {res.RegionName} - {res.City} ({res.Org})";
            }
        }

        return location;
    }
}