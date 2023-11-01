namespace Moonlight.App.Api;

public abstract class AbstractRequest
{
    public IServiceProvider ServiceProvider { get; set; }
    public ApiUserContext? Context { get; set; }
    public abstract void ReadData(RequestDataContext dataContext);
    public abstract Task ProcessRequest();
    public abstract ResponseDataBuilder CreateResponse(ResponseDataBuilder builder);
}