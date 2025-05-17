namespace Scripts.Models;

public class CsprojManifest
{
    public bool IsPackable { get; set; }
    public string Version { get; set; }
    public string PackageId { get; set; }
    public string[] PackageTags { get; set; }
}