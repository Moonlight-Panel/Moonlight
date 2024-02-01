using MoonCore.Attributes;
using MoonCore.Helpers;

namespace Moonlight.Core.Services;

[Singleton]
public class BucketService
{
    private readonly string BasePath;
    public string[] Buckets => GetBuckets();


    public BucketService()
    {
        // This is used to create the buckets folder in the persistent storage of helio
        BasePath = PathBuilder.Dir("storage", "buckets");
        Directory.CreateDirectory(BasePath);
    }

    public string[] GetBuckets()
    {
        return Directory
            .GetDirectories(BasePath)
            .Select(x =>
                x.Replace(BasePath, "").TrimEnd('/')
            )
            .ToArray();
    }

    public Task EnsureBucket(string name) // To ensure a specific bucket has been created, call this function
    {
        Directory.CreateDirectory(PathBuilder.Dir(BasePath, name));
        return Task.CompletedTask;
    }

    public async Task<string> Store(string bucket, Stream dataStream, string fileName)
    {
        await EnsureBucket(bucket); // Ensure the bucket actually exists

        // Create a safe to file name to store the file
        var extension = Path.GetExtension(fileName);
        var finalFileName = Path.GetRandomFileName() + extension;
        var finalFilePath = PathBuilder.File(BasePath, bucket, finalFileName);

        // Copy the file from the remote stream to the bucket
        var fs = File.Create(finalFilePath);
        await dataStream.CopyToAsync(fs);
        await fs.FlushAsync();
        fs.Close();
        
        // Return the generated file name to save it in the db or smth
        return finalFileName;
    }

    public Task<Stream> Pull(string bucket, string file)
    {
        var filePath = PathBuilder.File(BasePath, bucket, file);

        if (File.Exists(filePath))
        {
            var stream = File.Open(filePath, FileMode.Open);

            return Task.FromResult<Stream>(stream);
        }
        else
            throw new FileNotFoundException();
    }

    public Task Delete(string bucket, string file, bool ignoreNotFound = false)
    {
        var filePath = PathBuilder.File(BasePath, bucket, file);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return Task.CompletedTask;
        }

        // This section will only be reached if the file does not exist
        
        if (!ignoreNotFound)
            throw new FileNotFoundException();
        
        return Task.CompletedTask;
    }
}