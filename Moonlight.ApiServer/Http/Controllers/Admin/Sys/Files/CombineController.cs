using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoonCore.Helpers;
using Moonlight.ApiServer.Configuration;
using Moonlight.ApiServer.Helpers;
using Moonlight.Shared.Http.Requests.Admin.Sys.Files;

namespace Moonlight.ApiServer.Http.Controllers.Admin.Sys.Files;

[ApiController]
[Route("api/admin/system/files")]
[Authorize(Policy = "permissions:admin.system.files")]
public class CombineController : Controller
{
    private readonly AppConfiguration Configuration;

    private const string BaseDirectory = "storage";

    public CombineController(AppConfiguration configuration)
    {
        Configuration = configuration;
    }

    [HttpPost("combine")]
    public async Task<IResult> Combine([FromBody] CombineRequest request)
    {
        // Validate file lenght
        if (request.Files.Length < 2)
            return Results.Problem("At least two files are required", statusCode: 400);

        // Resolve the physical paths
        var destination = Path.Combine(BaseDirectory, FilePathHelper.SanitizePath(request.Destination));

        var files = request.Files
            .Select(path => Path.Combine(BaseDirectory, FilePathHelper.SanitizePath(path)))
            .ToArray();

        // Validate max file size
        long combinedSize = 0;

        foreach (var file in files)
        {
            var fi = new FileInfo(file);
            combinedSize += fi.Length;
        }

        if (ByteConverter.FromBytes(combinedSize).MegaBytes > Configuration.Files.CombineLimit)
        {
            return Results.Problem("The combine operation exceeds the maximum file size", statusCode: 400);
        }

        // Combine files

        await using var destinationFs = System.IO.File.Open(
            destination,
            FileMode.Create,
            FileAccess.ReadWrite,
            FileShare.Read
        );

        foreach (var file in files)
        {
            await using var fs = System.IO.File.Open(
                file,
                FileMode.Open,
                FileAccess.ReadWrite
            );

            await fs.CopyToAsync(destinationFs);
            await destinationFs.FlushAsync();

            fs.Close();
        }

        await destinationFs.FlushAsync();
        destinationFs.Close();

        return Results.Ok();
    }
}