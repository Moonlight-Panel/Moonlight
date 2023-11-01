using System.Text;

namespace Moonlight.App.Api;

public class ResponseDataBuilder
{
    private List<byte> Data;

    public ResponseDataBuilder()
    {
        Data = new List<byte>();
    }

    public void WriteInt(int data)
    {
        var bytes = BitConverter.GetBytes(data);

        if (BitConverter.IsLittleEndian) // because of java (the app needing th api is written in java/kotlin) we need to use big endian
        {
            bytes = bytes.Reverse().ToArray();
        }
        
        Data.AddRange(bytes);
    }

    public void WriteByte(byte data)
    {
        Data.Add(data);
    }

    public void WriteBoolean(bool data)
    {
        WriteByte(data ? (byte)255 : (byte)0);
    }

    public void WriteString(String data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        var len = bytes.Length;
        
        WriteInt(len);
        Data.AddRange(bytes);
    }

    public byte[] ToBytes()
    {
        return Data.ToArray();
    }
}