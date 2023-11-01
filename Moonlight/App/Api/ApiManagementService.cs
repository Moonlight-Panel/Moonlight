using System.Net.WebSockets;
using System.Reflection;

namespace Moonlight.App.Api;

public class ApiManagementService
{
    public Dictionary<int, Type> Requests;
    public List<ApiUserContext> Contexts;

    public ApiManagementService()
    {
        Requests = new Dictionary<int, Type>();
        Contexts = new List<ApiUserContext>();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            var types = assembly.ExportedTypes;

            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<ApiRequestAttribute>();
                
                if(attribute == null)
                    continue;

                var id = attribute.Id;
                Requests[id] = type;
            }
        }
    }

    public AbstractRequest GetRequest(int id, ApiUserContext context)
    {
        var type = Requests[id];
        var obj = Activator.CreateInstance(type) as AbstractRequest;
        obj!.Context = context;

        return obj!;
    }

    public async Task HandleRequest(ApiUserContext context, byte[] data)
    {
        var rqd = new RequestDataContext(data);
        var id = rqd.ReadInt();
        var request = GetRequest(id, context);
        
        request.ReadData(rqd);
        request.ProcessRequest();

        var rbd = new ResponseDataBuilder();
        rbd = request.CreateResponse(rbd);

        CancellationToken t = new CancellationToken();
        var bytes = rbd.ToBytes();
        await context.WebSocket.SendAsync(bytes, WebSocketMessageType.Binary, true, t);
    }
}