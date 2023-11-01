using System.Buffers.Binary;
using System.Text;

namespace Moonlight.App.Api;

public class RequestDataContext
{
    private List<byte> Data;
    
    public RequestDataContext(byte[] data)
    {
        Data = data.ToList();
    }

    public int ReadInt()
    {
        var bytes = Data.Take(4).ToList();
        Data.RemoveRange(0, 4);

        if (BitConverter.IsLittleEndian) // because of java (the app needing the api is written in java/kotlin) we need to use big endian
        {
            bytes.Reverse();
        }

        return BitConverter.ToInt32(bytes.ToArray());
    }

    public byte ReadByte()
    {
        var b = Data[0];
        Data.RemoveAt(0);
        return b;
    }

    public bool ReadBoolean()
    {
        var b = ReadByte();

        return b == 255;
    }

    public String ReadString()
    {
        var len = ReadInt();

        var bytes = Data.Take(len).ToList();
        Data.RemoveRange(0, len);

        return Encoding.UTF8.GetString(bytes.ToArray());
    }
}