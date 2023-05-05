using Moonlight.App.Helpers;

namespace Moonlight.App.Services.Files;

public class BucketService
{
    private string BucketPath;

    public BucketService()
    {
        BucketPath = PathBuilder.Dir("storage", "uploads");
    }

    public Task<string[]> GetBuckets()
    {
        var buckets = Directory.GetDirectories(BucketPath)
            .Select(x =>
                x.Replace(BucketPath, "").TrimEnd('/')
            )
            .ToArray();

        return Task.FromResult(buckets);
    }

    private Task EnsureBucket(string name)
    {
        Directory.CreateDirectory(BucketPath + name);

        return Task.CompletedTask;
    }

    public async Task<string> StoreFile(string bucket, Stream dataStream, string? name = null)
    {
        await EnsureBucket(bucket);

        var extension = "";

        if (name != null)
            extension = Path.GetExtension(name);

        var fileName = Path.GetRandomFileName() + extension; //TODO: Add check for existing file
        var filePath = BucketPath + PathBuilder.File(bucket, fileName);
        
        var fileStream = File.Create(filePath);

        await dataStream.CopyToAsync(fileStream);
        await fileStream.FlushAsync();

        fileStream.Close();

        return fileName;
    }

    public Task<Stream> GetFile(string bucket, string file)
    {
        var filePath = BucketPath + PathBuilder.File(bucket, file);

        if (File.Exists(filePath))
        {
            var stream = File.Open(filePath, FileMode.Open);

            return Task.FromResult<Stream>(stream);
        }
        else
            throw new FileNotFoundException();
    }
}