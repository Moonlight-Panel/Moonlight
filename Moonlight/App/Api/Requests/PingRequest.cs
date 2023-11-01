namespace Moonlight.App.Api.Requests;

[ApiRequest(1)]
public class PingRequest : AbstractRequest
{
    public override void ReadData(RequestDataContext dataContext)
    {
        var chunk = dataContext.ReadInt();
    }

    public override ResponseDataBuilder CreateResponse(ResponseDataBuilder builder)
    {
        builder.WriteInt(10324);
        return builder;
    }

    public override void ProcessRequest()
    {
        
    }
}