using Microsoft.AspNetCore.Components;
using Moonlight.App.Helpers;

namespace Moonlight.App.Services
{
    public class TranslationService
    {
        private string LanguageCode { get; set; } = "en_us";
        private readonly string FallbackLanguage = "en_us";

        private TranslationHelper Helper { get; }
        private HttpContext HttpContext { get; }

        public TranslationService(TranslationHelper helper, IHttpContextAccessor httpContextAccessor)
        {
            Helper = helper;
            HttpContext = httpContextAccessor.HttpContext!;

            LanguageCode = FallbackLanguage;

            try
            {
                var langHeader = HttpContext.Request.Headers["Accept-Language"].ToString();
                var important = langHeader.Split(";").First().ToLower().Split(",");
                
                foreach (var v in important)
                {
                    if (v.Contains("de"))
                    {
                        LanguageCode = "de_de";
                        break;
                    }
                    else if (v.Contains("en"))
                    {
                        LanguageCode = "en_us";
                        break;
                    }
                }

                var cookies = HttpContext.Request.Headers["Cookie"].ToString().Split(new string[] { ",", ";" }, StringSplitOptions.TrimEntries);

                foreach (var cookie in cookies)
                {
                    var name = cookie.Split("=").First().Trim();
                    var value = cookie.Split("=").Last().Trim();

                    if (name == "lang")
                    {
                        if (value.Contains("de"))
                        {
                            LanguageCode = "de_de";
                            break;
                        }
                        else if (value.Contains("en"))
                        {
                            LanguageCode = "en_us";
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        public string Translate(string key, params object[] parameters)
        {
            try
            {
                var text = Helper.Get(LanguageCode, key);

                if (text == null)
                    return key;

                int i = 0;
                foreach (var param in parameters)
                {
                    text = text.Replace($"{{{i}}}", param.ToString());
                    i++;
                }

                return text;
            }
            catch (Exception ex)
            {
                Logging.Net.Logger.Error(ex);
                return key;
            }
        }

        public MarkupString TranslateMarkup(string key, params object[] parameters)
        {
            return new MarkupString(Translate(key, parameters));
        }
    }
}
