namespace Moonlight.App.Helpers;

public class ByteSizeValue
{
    public long Bytes { get; set; }

    public long KiloBytes
    {
        get => Bytes / 1024;
        set => Bytes = value * 1024;
    }
    
    public long MegaBytes
    {
        get => KiloBytes / 1024;
        set => KiloBytes = value * 1024;
    }
    
    public long GigaBytes
    {
        get => MegaBytes / 1024;
        set => MegaBytes = value * 1024;
    }

    public static ByteSizeValue FromBytes(long bytes)
    {
        return new()
        {
            Bytes = bytes
        };
    }
    
    public static ByteSizeValue FromKiloBytes(long kiloBytes)
    {
        return new()
        {
            KiloBytes = kiloBytes
        };
    }
    
    public static ByteSizeValue FromMegaBytes(long megaBytes)
    {
        return new()
        {
            MegaBytes = megaBytes
        };
    }
    
    public static ByteSizeValue FromGigaBytes(long gigaBytes)
    {
        return new()
        {
            GigaBytes = gigaBytes
        };
    }
}