using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

// Handle arguments
if (Args.Count != 1)
{
    Console.WriteLine("You need to provide the path to a nuget file as a parameter");
    return;
}

var nugetPath = Args[0];

// Check if file exists
if (!File.Exists(nugetPath))
{
    Console.WriteLine("The provided file does not exist");
    return;
}

// Open file to modify
Console.WriteLine($"Modding nuget package: {nugetPath}");
var zipFile = ZipFile.Open(nugetPath, ZipArchiveMode.Update, Encoding.UTF8);

// First we want to remove the framework files
Console.WriteLine("Removing framework files");

var frameworkEntries = zipFile.Entries
    .Where(x => x.FullName.Contains("staticwebassets/_framework"))
    .ToArray();

foreach (var frameworkEntry in frameworkEntries)
    frameworkEntry.Delete();
    
// Then we want to modify the build targets for static web assets
var oldBuildTarget = zipFile.Entries
    .First(x => x.FullName == "build/Microsoft.AspNetCore.StaticWebAssets.props");

// Load old content
var oldContentStream = oldBuildTarget.Open();

// Parse xml and remove framework references
Console.WriteLine("Removing framework web asset references");

var contentXml = XDocument.Load(oldContentStream);
oldContentStream.Close();
oldContentStream.Dispose();
oldBuildTarget.Delete();

var assetsToRemove = contentXml
    .Descendants("StaticWebAsset")
    .Where(asset =>
        asset.Element("RelativePath")?.Value.Contains("_framework") == true)
    .ToArray();

foreach (var asset in assetsToRemove)
    asset.Remove();

var newBuildTarget = zipFile.CreateEntry("build/Microsoft.AspNetCore.StaticWebAssets.props");
var newContentStream = newBuildTarget.Open();
contentXml.Save(newContentStream);

await newContentStream.FlushAsync();
newContentStream.Close();

zipFile.Dispose();