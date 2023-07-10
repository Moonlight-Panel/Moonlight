using System.Text;
using Moonlight.App.Database.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Moonlight.App.Helpers;

public static class EggConverter
{
    public static Image Convert(string json)
    {
        var result = new Image();

        var data = new ConfigurationBuilder().AddJsonStream(
            new MemoryStream(Encoding.ASCII.GetBytes(json))
        ).Build();

        result.Allocations = 1;
        result.Description = data.GetValue<string>("description") ?? "";
        result.Uuid = Guid.NewGuid();
        result.Startup = data.GetValue<string>("startup") ?? "";
        result.Name = data.GetValue<string>("name") ?? "Ptero Egg";

        foreach (var variable in data.GetSection("variables").GetChildren())
        {
            result.Variables.Add(new()
            {
                Key = variable.GetValue<string>("env_variable") ?? "",
                DefaultValue = variable.GetValue<string>("default_value") ?? ""
            });
        }
        
        var configData = data.GetSection("config");

        result.ConfigFiles = configData.GetValue<string>("files") ?? "{}";

        var dImagesData = JObject.Parse(json);
        var dImages = (JObject)dImagesData["docker_images"]!;
        
        foreach (var dockerImage in dImages)
        {
            var di = new DockerImage()
            {
                Default = dockerImage.Key == dImages.Properties().Last().Name,
                Name = dockerImage.Value!.ToString()
            };

            result.DockerImages.Add(di);
        }

        var installSection = data.GetSection("scripts").GetSection("installation");

        result.InstallEntrypoint = installSection.GetValue<string>("entrypoint") ?? "bash";
        result.InstallScript = installSection.GetValue<string>("script") ?? "";
        result.InstallDockerImage = installSection.GetValue<string>("container") ?? "";

        var rawJson = configData.GetValue<string>("startup");
        
        var startupData = new ConfigurationBuilder().AddJsonStream(
            new MemoryStream(Encoding.ASCII.GetBytes(rawJson!))
        ).Build();

        result.StartupDetection = startupData.GetValue<string>("done", "") ?? "";
        result.StopCommand = configData.GetValue<string>("stop") ?? "";

        result.TagsJson = "[]";
        result.BackgroundImageUrl = "";

        return result;
    }
}