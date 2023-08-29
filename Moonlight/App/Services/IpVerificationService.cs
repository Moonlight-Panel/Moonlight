using Moonlight.App.Helpers;
using Whois.NET;

namespace Moonlight.App.Services;

public class IpVerificationService
{
    private readonly ConfigService ConfigService;

    public IpVerificationService(ConfigService configService)
    {
        ConfigService = configService;
    }

    public async Task<bool> IsDatacenterOrVpn(string ip)
    {
        if (!ConfigService.Get().Moonlight.Security.BlockDatacenterIps)
            return false;
        
        if (string.IsNullOrEmpty(ip))
            return false;

        var datacenterNames = new List<string>()
        {
            "amazon",
            "aws",
            "microsoft",
            "azure",
            "google",
            "google cloud",
            "gcp",
            "digitalocean",
            "linode",
            "vultr",
            "ovh",
            "ovhcloud",
            "alibaba",
            "oracle",
            "ibm cloud",
            "bluehost",
            "godaddy",
            "rackpace",
            "hetzner",
            "tencent",
            "scaleway",
            "softlayer",
            "dreamhost",
            "a2 hosting",
            "inmotion hosting",
            "red hat openstack",
            "kamatera",
            "hostgator",
            "siteground",
            "greengeeks",
            "liquidweb",
            "joyent",
            "aruba",
            "interoute",
            "fastcomet",
            "rosehosting",
            "lunarpages",
            "fatcow",
            "jelastic",
            "datacamp"
        };
        
        if(!ConfigService.Get().Moonlight.Security.AllowCloudflareIps)
            datacenterNames.Add("cloudflare");

        try
        {
            var response = await WhoisClient.QueryAsync(ip);
            var responseText = response.Raw.ToLower();

            foreach (var name in datacenterNames)
            {
                if (responseText.Contains(name))
                {
                    Logger.Debug(name);
                    return true;
                }
            }
        }
        catch (Exception)
        {
            return false;
        }

        return false;
    }
}