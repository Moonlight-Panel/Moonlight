namespace Moonlight.App.Helpers;

public class StreamProgressHelper : Stream
{
    public Action<int>? Progress { get; set; }
    private int lastPercent = -1;

    Stream InnerStream { get; init; }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var result = InnerStream.ReadAsync(buffer, offset, count).Result;

        int percentComplete = (int)Math.Round((double)(100 * Position) / Length);

        if (lastPercent != percentComplete)
        {
            Progress?.Invoke(percentComplete);
            lastPercent = percentComplete;
        }
        
        return result;
    }
    public override void Write(byte[] buffer, int offset, int count)
    {
        InnerStream.WriteAsync(buffer, offset, count);
    }
    public override bool CanRead => InnerStream.CanRead;
    public override bool CanSeek => InnerStream.CanSeek;
    public override bool CanWrite => InnerStream.CanWrite;
    public override long Length => InnerStream.Length;
    public override long Position { get => InnerStream.Position; set => InnerStream.Position = value; }
    public StreamProgressHelper(Stream s)
    {
        this.InnerStream = s;
    }
    public override void Flush() => InnerStream.Flush();
    public override long Seek(long offset, SeekOrigin origin) => InnerStream.Seek(offset, origin);
    public override void SetLength(long value)=> InnerStream.SetLength(value);
}