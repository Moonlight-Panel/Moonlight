namespace Moonlight.App.Api;

public abstract class AbstractRequest
{
    public ApiUserContext? Context { get; set; }
    public abstract void ReadData(RequestDataContext dataContext);
    public abstract void ProcessRequest();
    public abstract ResponseDataBuilder CreateResponse(ResponseDataBuilder builder);
}