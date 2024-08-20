namespace Moonlight.ApiServer.App.Services;

public class ApplicationService
{
    private WebApplication Application;
    private readonly ILogger<ApplicationService> Logger;
    
    public ApplicationService(ILogger<ApplicationService> logger)
    {
        Logger = logger;
    }

    public void SetApplication(WebApplication application) => Application = application;
    
    public Task Shutdown()
    {
        Logger.LogInformation("Shutdown of api server requested");

        Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            await Application.StopAsync();
        });
        
        return Task.CompletedTask;
    }
}