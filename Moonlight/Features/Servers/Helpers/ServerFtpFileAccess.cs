using System.Net;
using System.Text;
using FluentFTP;
using MoonCore.Helpers;
using Moonlight.Features.FileManager.Models.Abstractions.FileAccess;

namespace Moonlight.Features.Servers.Helpers;

public class ServerFtpFileAccess : IFileAccess
{
    private FtpClient Client;
    private string CurrentDirectory = "/";

    private readonly string Host;
    private readonly int Port;
    private readonly string Username;
    private readonly string Password;
    private readonly int OperationTimeout;

    public ServerFtpFileAccess(string host, int port, string username, string password, int operationTimeout = 5)
    {
        Host = host;
        Port = port;
        Username = username;
        Password = password;
        OperationTimeout = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;

        Client = CreateClient();
    }

    public async Task<FileEntry[]> List()
    {
        return await ExecuteHandled(() =>
        {
            var items = Client.GetListing() ?? Array.Empty<FtpListItem>();
            var result = items.Select(item => new FileEntry
            {
                Name = item.Name,
                IsDirectory = item.Type == FtpObjectType.Directory,
                IsFile = item.Type == FtpObjectType.File,
                LastModifiedAt = item.Modified,
                Size = item.Size
            }).ToArray();
            
            return Task.FromResult(result);
        });
    }

    public async Task ChangeDirectory(string relativePath)
    {
        await ExecuteHandled(() =>
        {
            var newPath = Path.Combine(CurrentDirectory, relativePath);
            newPath = Path.GetFullPath(newPath);

            Client.SetWorkingDirectory(newPath);
            CurrentDirectory = Client.GetWorkingDirectory();

            return Task.CompletedTask;
        });
    }

    public async Task SetDirectory(string path)
    {
        await ExecuteHandled(() =>
        {
            Client.SetWorkingDirectory(path);
            CurrentDirectory = Client.GetWorkingDirectory();
            
            return Task.CompletedTask;
        });
    }

    public Task<string> GetCurrentDirectory()
    {
        return Task.FromResult(CurrentDirectory);
    }

    public async Task Delete(string path)
    {
        await ExecuteHandled(() =>
        {
            if (Client.FileExists(path))
                Client.DeleteFile(path);
            else
                Client.DeleteDirectory(path);

            return Task.CompletedTask;
        });
    }

    public async Task Move(string from, string to)
    {
        await ExecuteHandled(() =>
        {
            var fromEntry = Client.GetObjectInfo(from);

            var dest = to + Path.GetFileName(from);
            var fromWithSlash = from.StartsWith("/") ? from : "/" + from;
            
            if (fromWithSlash == dest)
                return Task.CompletedTask;

            if (fromEntry.Type == FtpObjectType.Directory)
                // We need to add the folder name here, because some ftp servers would refuse to move the folder if its missing
                Client.MoveDirectory(from, dest);
            else
                // We need to add the file name here, because some ftp servers would refuse to move the file if its missing
                Client.MoveFile(from, dest);
            
            return Task.CompletedTask;
        });
    }

    public async Task CreateDirectory(string name)
    {
        await ExecuteHandled(() =>
        {
            Client.CreateDirectory(name);
            return Task.CompletedTask;
        });
    }

    public async Task CreateFile(string name)
    {
        await ExecuteHandled(() =>
        {
            using var stream = new MemoryStream();
            Client.UploadStream(stream, name);

            return Task.CompletedTask;
        });
    }

    public async Task<string> ReadFile(string name)
    {
        return await ExecuteHandled(async () =>
        {
            await using var stream = Client.OpenRead(name);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        });
    }

    public async Task WriteFile(string name, string content)
    {
        await ExecuteHandled(() =>
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            Client.UploadStream(stream, name);

            return Task.CompletedTask;
        });
    }

    public async Task<Stream> ReadFileStream(string name)
    {
        return await ExecuteHandled(() =>
        {
            var stream = Client.OpenRead(name);
            return Task.FromResult(stream);
        });
    }

    public async Task WriteFileStream(string name, Stream dataStream)
    {
        await ExecuteHandled(() =>
        {
            Client.UploadStream(dataStream, name, FtpRemoteExists.Overwrite);
            return Task.CompletedTask;
        });
    }
    
    public IFileAccess Clone()
    {
        return new ServerFtpFileAccess(Host, Port, Username, Password)
        {
            CurrentDirectory = CurrentDirectory
        };
    }

    public void Dispose()
    {
        Client.Dispose();
    }

    #region Helpers

    private Task EnsureConnected()
    {
        if (!Client.IsConnected)
        {
            Client.Connect();
            
            // This will set the correct current directory
            // on cloned or reconnected FtpFileAccess instances
            if(CurrentDirectory != "/")
                Client.SetWorkingDirectory(CurrentDirectory);
        }

        return Task.CompletedTask;
    }
    
    private async Task ExecuteHandled(Func<Task> func)
    {
        try
        {
            await EnsureConnected();
            await func.Invoke();
        }
        catch (TimeoutException)
        {
            Client.Dispose();
            Client = CreateClient();

            await EnsureConnected();
            
            await func.Invoke();
        }
    }
    
    private async Task<T> ExecuteHandled<T>(Func<Task<T>> func)
    {
        try
        {
            await EnsureConnected();
            return await func.Invoke();
        }
        catch (TimeoutException)
        {
            Client.Dispose();
            Client = CreateClient();

            await EnsureConnected();

            return await func.Invoke();
        }
    }
    
    

    private FtpClient CreateClient()
    {
        var client = new FtpClient();
        client.Host = Host;
        client.Port = Port;
        client.Credentials = new NetworkCredential(Username, Password);
        client.Config.DataConnectionType = FtpDataConnectionType.PASV;

        client.Config.ConnectTimeout = OperationTimeout;
        client.Config.ReadTimeout = OperationTimeout;
        client.Config.DataConnectionConnectTimeout = OperationTimeout;
        client.Config.DataConnectionReadTimeout = OperationTimeout;

        return client;
    }

    #endregion
}
