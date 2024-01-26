using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace Moonlight.Core.Helpers;

public class WsPacketConnection
{
    private readonly Dictionary<string, Type> Packets = new();
    private readonly WebSocket WebSocket;

    public WsPacketConnection(WebSocket webSocket)
    {
        WebSocket = webSocket;
    }

    public Task RegisterPacket<T>(string id)
    {
        lock (Packets)
            Packets.Add(id, typeof(T));

        return Task.CompletedTask;
    }

    public async Task Send(object packet)
    {
        string? packetId = null;

        // Search packet registration
        lock (Packets)
        {
            if (Packets.Any(x => x.Value == packet.GetType()))
                packetId = Packets.First(x => x.Value == packet.GetType()).Key;

            if (packetId == null)
                throw new ArgumentException($"A packet with the type {packet.GetType().FullName} is not registered");
        }

        // Build raw packet
        var rawPacket = new RawPacket()
        {
            Id = packetId,
            Data = packet
        };

        // Serialize, encode and build buffer
        var json = JsonConvert.SerializeObject(rawPacket);
        var buffer = Encoding.UTF8.GetBytes(json);

        await WebSocket.SendAsync(buffer, WebSocketMessageType.Text, WebSocketMessageFlags.None,
            CancellationToken.None);
    }

    public async Task<object?> Receive()
    {
        // Build buffer and read
        var buffer = new byte[1024];
        await WebSocket.ReceiveAsync(buffer, CancellationToken.None);

        // Decode and deserialize
        var json = Encoding.UTF8.GetString(buffer);
        var rawPacket = JsonConvert.DeserializeObject<RawPacket>(json)!;

        object? packetType = null;

        // Search packet registration
        lock (Packets)
        {
            if (Packets.ContainsKey(rawPacket.Id))
                packetType = Packets[rawPacket.Id];

            if (packetType == null)
                throw new ArgumentException($"A packet with the type {rawPacket.Id} is not registered");
        }

        var typedPacketType = typeof(RawPacket<>).MakeGenericType((packetType as Type)!);
        var typedPacket = JsonConvert.DeserializeObject(json, typedPacketType);
        
        return typedPacketType.GetProperty("Data")!.GetValue(typedPacket);
    }

    public async Task Close()
    {
        if(WebSocket.State == WebSocketState.Open) 
            await WebSocket.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);
    }

    public class RawPacket
    {
        public string Id { get; set; }
        public object Data { get; set; }
    }

    public class RawPacket<T>
    {
        public string Id { get; set; }
        public T Data { get; set; }
    }
}