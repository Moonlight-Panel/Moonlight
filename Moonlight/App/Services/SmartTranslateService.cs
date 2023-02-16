using Moonlight.App.Helpers;

namespace Moonlight.App.Services;

public class SmartTranslateService
{
    private string LanguageCode { get; set; } = "en_us";
    private readonly string FallbackLanguage = "en_us";

    private SmartTranslateHelper Helper { get; }
    private HttpContext HttpContext { get; }

    public SmartTranslateService(SmartTranslateHelper helper, IHttpContextAccessor httpContextAccessor)
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

            var cookies = HttpContext.Request.Headers["Cookie"].ToString()
                .Split(new string[] { ",", ";" }, StringSplitOptions.TrimEntries);

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
            // ignored
        }
    }

    public string Translate(string key)
    {
        try
        {
            var text = Helper.Translate(LanguageCode, key);

            if (text == null)
                return key;

            return text;
        }
        catch (Exception ex)
        {
            Logging.Net.Logger.Error(ex);
            return key;
        }
    }
}