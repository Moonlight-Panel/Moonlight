namespace Moonlight.ApiSdk.Features.Misc;

public class RangedData<T>
{
    public RangedData(IReadOnlyList<T> data, int totalLength)
    {
        Data = data;
        TotalLength = totalLength;
    }

    public IReadOnlyList<T> Data { get; set; }
    public int TotalLength { get; set; }
}