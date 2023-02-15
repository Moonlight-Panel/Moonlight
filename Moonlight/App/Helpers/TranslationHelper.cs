using Logging.Net;
using Newtonsoft.Json;

namespace Moonlight.App.Helpers
{
    public class TranslationHelper
    {
        private readonly Dictionary<string, IConfiguration> Languages;

        public TranslationHelper()
        {
            Languages = new();

            foreach (var file in Directory.GetFiles("resources/lang"))
            {
                var langKey = Path.GetFileName(file)
                    .Replace(Path.GetExtension(file), "");
                
                Languages.Add(langKey, new ConfigurationBuilder()
                    .AddJsonFile(file)
                    .Build());
            }
        }

        public string? Get(string langKey, string key)
        {
            if (Languages.ContainsKey(langKey))
            {
                var parts = key.Split(".");

                IConfigurationSection? section = null;
                foreach (var part in parts)
                {
                    if (part == parts.Last())
                    {
                        if (section == null)
                            return Languages[langKey].GetValue<string>(part);
                        else
                            return section.GetValue<string>(part);
                    }
                    else
                    {
                        if (section == null)
                            section = Languages[langKey].GetSection(part);
                        else
                            section = section.GetSection(part);
                    }
                }

                return key;
            }
            else
            {
                return "Invalid lang key";
            }
        }
    }
}
