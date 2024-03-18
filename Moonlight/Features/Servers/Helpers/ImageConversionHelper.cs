using System.Text.RegularExpressions;
using Mappy.Net;
using Microsoft.EntityFrameworkCore;
using MoonCore.Abstractions;
using MoonCore.Attributes;
using MoonCore.Exceptions;
using MoonCore.Extensions;
using Moonlight.Features.Servers.Entities;
using Moonlight.Features.Servers.Models;
using Moonlight.Features.Servers.Models.Json;
using Newtonsoft.Json;

namespace Moonlight.Features.Servers.Helpers;

[Scoped]
public class ImageConversionHelper
{
    private readonly Repository<ServerImage> ImageRepository;

    public ImageConversionHelper(Repository<ServerImage> imageRepository)
    {
        ImageRepository = imageRepository;
    }

    public Task<string> ExportAsJson(ServerImage image)
    {
        var imageWithData = ImageRepository
            .Get()
            .Include(x => x.DockerImages)
            .Include(x => x.Variables)
            .First(x => x.Id == image.Id);

        
        var model = new ImageJson()
        {
            Name = imageWithData.Name,
            Author = imageWithData.Author,
            AllocationsNeeded = imageWithData.AllocationsNeeded,
            DonateUrl = imageWithData.DonateUrl,
            InstallScript = imageWithData.InstallScript,
            InstallShell = imageWithData.InstallShell,
            OnlineDetection = imageWithData.OnlineDetection,
            ParseConfiguration = imageWithData.ParseConfiguration,
            StartupCommand = imageWithData.StartupCommand,
            StopCommand = imageWithData.StopCommand,
            UpdateUrl = imageWithData.UpdateUrl,
            DefaultDockerImage = imageWithData.DefaultDockerImage,
            InstallDockerImage = imageWithData.InstallDockerImage,
            AllowDockerImageChange = imageWithData.AllowDockerImageChange
        };

        // This wont work, mappy.net does support nesting nor ignoring attributes
        // var model = Mapper.Map<ImageJson>(imageWithData);

        foreach (var variable in imageWithData.Variables)
            model.Variables.Add(Mapper.Map<ImageJson.ImageVariable>(variable));
        
        foreach (var dockerImage in imageWithData.DockerImages)
            model.DockerImages.Add(Mapper.Map<ImageJson.ImageDockerImage>(dockerImage));

        return Task.FromResult(JsonConvert.SerializeObject(model, Formatting.Indented));
    }

    public Task<ServerImage> ImportFromJson(string json)
    {
        var model = JsonConvert.DeserializeObject<ImageJson>(json);

        if (model == null)
            throw new DisplayException("Unable to deserialize image json");

        var result = new ServerImage()
        {
            Name = model.Name,
            Author = model.Author,
            AllocationsNeeded = model.AllocationsNeeded,
            DonateUrl = model.DonateUrl,
            InstallScript = model.InstallScript,
            InstallShell = model.InstallShell,
            StartupCommand = model.StartupCommand,
            StopCommand = model.StopCommand,
            UpdateUrl = model.UpdateUrl,
            DefaultDockerImage = model.DefaultDockerImage,
            InstallDockerImage = model.InstallDockerImage,
            AllowDockerImageChange = model.AllowDockerImageChange,
            OnlineDetection = model.OnlineDetection,
            ParseConfiguration = model.ParseConfiguration
        };

        foreach (var variable in model.Variables)
            result.Variables.Add(Mapper.Map<ServerImageVariable>(variable));
        
        foreach (var dockerImage in model.DockerImages)
            result.DockerImages.Add(Mapper.Map<ServerDockerImage>(dockerImage));

        return Task.FromResult(result);
    }

    public Task<ServerImage> ImportFromEggJson(string json)
    {
        var fixedJson = json;

        fixedJson = fixedJson.Replace("\\/", "/");
        
        // Note: We use microsofts config system here as its dynamic and probably the best parsing method to use here
        var eggData = new ConfigurationBuilder()
            .AddJsonString(fixedJson)
            .Build();

        var result = new ServerImage();

        result.AllocationsNeeded = 1; // We cannot convert this value as its moonlight native

        result.Name = eggData["name"] ?? "Name was missing";
        result.Author = eggData["author"] ?? "Author was missing";
        result.StartupCommand = eggData["startup"] ?? "Startup was missing";
        result.StopCommand = eggData.GetSection("config")["stop"] ?? "Stop command was missing";
        
        // Startup detection
        var startupDetectionData = new ConfigurationBuilder()
            .AddJsonString(eggData.GetSection("config")["startup"] ?? "{}")
            .Build();

        // Node Regex: As the online detection uses regex, we want to escape any special chars from egg imports
        // as eggs dont use regex and as such may contain characters which regex uses as meta characters.
        // Without this escaping, many startup detection string wont work
        result.OnlineDetection = Regex.Escape(startupDetectionData["done"] ?? "Online detection was missing");

        // Docker image method 1:
        // Reference egg: https://github.com/parkervcp/eggs/blob/master/game_eggs/mindustry/egg-mindustry.json
        if (eggData["image"] != null)
        {
            result.DockerImages.Add(new ()
            {
                Name = eggData["image"]!,
                AutoPull = true,
                DisplayName = eggData["image"]!,
            });
        }

        // Docker image method 2:
        // Reference egg: https://github.com/parkervcp/eggs/blob/master/game_eggs/minecraft/java/paper/egg-paper.json
        var dockerImagesSection = eggData.GetSection("docker_images");

        foreach (var dockerImage in dockerImagesSection.GetChildren())
        {
            result.DockerImages.Add(new ()
            {
                Name = dockerImage.Value ?? "Docker image name was missing",
                AutoPull = true,
                DisplayName = dockerImage.Key
            });
        }
        
        // Docker image method 3:
        // Reference egg: https://github.com/parkervcp/eggs/blob/master/game_eggs/minecraft/java/cuberite/egg-cuberite.json
        var dockerImagesList = eggData.GetValue<List<string>>("images");

        if (dockerImagesList != null)
        {
            foreach (var imageName in dockerImagesList)
            {
                result.DockerImages.Add(new ()
                {
                    Name = imageName,
                    AutoPull = true,
                    DisplayName = imageName,
                });
            }
        }
        
        // Parse config
        var parseConfigData = new ConfigurationBuilder()
            .AddJsonString(eggData.GetSection("config")["files"] ?? "{}")
            .Build();

        var parseConfigModels = new List<ServerParseConfig>();
        
        foreach (var fileSection in parseConfigData.GetChildren())
        {
            var model = new ServerParseConfig()
            {
                File = fileSection.Key,
                Type = fileSection["parser"] ?? "parser was missing"
            };

            foreach (var findSection in fileSection.GetSection("find").GetChildren())
            {
                var valueWithChecks = findSection.Value ?? "Find value was null";

                valueWithChecks = valueWithChecks.Replace("server.build.default.port", "SERVER_PORT");
                valueWithChecks = valueWithChecks.Replace("server.build.env.", "");
                
                model.Configuration.Add(findSection.Key, valueWithChecks);
            }
            
            parseConfigModels.Add(model);
        }

        result.ParseConfiguration = JsonConvert.SerializeObject(parseConfigModels);
        
        // Installation
        result.InstallShell = "/bin/" + (eggData.GetSection("scripts").GetSection("installation")["entrypoint"] ?? "Install shell was missing");
        result.InstallScript = eggData.GetSection("scripts").GetSection("installation")["script"] ?? "Install script was missing";
        result.InstallDockerImage = eggData.GetSection("scripts").GetSection("installation")["container"] ?? "Install script was missing";
        
        // Variables
        foreach (var variableSection in eggData.GetSection("variables").GetChildren())
        {
            var variable = new ServerImageVariable()
            {
                DisplayName = variableSection["name"] ?? "Name was missing",
                Description = variableSection["description"] ?? "Description was missing",
                Key = variableSection["env_variable"] ?? "Environment variable was missing",
                DefaultValue = variableSection["default_value"] ?? "Default value was missing",
            };

            // Check if it is a bool value
            if (bool.TryParse(variableSection["user_viewable"], out _))
            {
                variable.AllowView = variableSection.GetValue<bool>("user_viewable");
                variable.AllowEdit = variableSection.GetValue<bool>("user_editable");
            }
            else
            {
                variable.AllowView = variableSection.GetValue<int>("user_viewable") == 1;
                variable.AllowEdit = variableSection.GetValue<int>("user_editable") == 1;
            }
            
            result.Variables.Add(variable);
        }
        
        return Task.FromResult(result);
    }
}