namespace Moonlight.ApiServer.Services;

public class BundleService
{
    private readonly List<string> CssFiles = new();

    public void BundleCss(string path)
        => CssFiles.Add(path);

    public IEnumerable<string> GetCssFiles() => CssFiles;
}