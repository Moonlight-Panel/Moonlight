using System.Text;

namespace Moonlight.App.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddJsonString(this IConfigurationBuilder configurationBuilder, string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        var stream = new MemoryStream(bytes);
        return configurationBuilder.AddJsonStream(stream);
    }
}