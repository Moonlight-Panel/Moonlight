using System.Data;
using Moonlight.App.Helpers;

namespace Moonlight.App.Perms;

public class PermissionStorage
{
    public byte[] Data;
    public bool IsReadyOnly;

    public PermissionStorage(byte[] data, bool isReadyOnly = false)
    {
        Data = data;
        IsReadyOnly = isReadyOnly;
    }

    public bool this[Permission permission]
    {
        get
        {
            try
            {
                return BitHelper.ReadBit(Data, permission.Index);
            }
            catch (Exception e)
            {
                Logger.Verbose("Error reading permissions. (Can be intentional)");
                Logger.Verbose(e);
                return false;
            }
        }
        set
        {
            if (IsReadyOnly)
                throw new ReadOnlyException();

            Data = BitHelper.WriteBit(Data, permission.Index, value);
        }
    }
}