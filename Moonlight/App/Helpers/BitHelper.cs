namespace Moonlight.App.Helpers;

public class BitHelper
{
    public static bool ReadBit(byte[] byteArray, int bitIndex)
    {
        if (bitIndex < 0)
            throw new ArgumentOutOfRangeException("bitIndex");

        int byteIndex = bitIndex / 8;
        if (byteIndex >= byteArray.Length)
            throw new ArgumentOutOfRangeException("bitIndex");

        int bitNumber = bitIndex % 8;
        byte mask = (byte)(1 << bitNumber);

        return (byteArray[byteIndex] & mask) != 0;
    }
    
    public static byte[] WriteBit(byte[] byteArray, int bitIndex, bool value)
    {
        if (bitIndex < 0)
            throw new ArgumentOutOfRangeException("bitIndex");

        int byteIndex = bitIndex / 8;
        byte[] resultArray;

        if (byteIndex >= byteArray.Length)
        {
            // Create a new array with increased size and copy elements from old array
            resultArray = new byte[byteIndex + 1];
            Array.Copy(byteArray, resultArray, byteArray.Length);
        }
        else
        {
            // Create a new array and copy elements from old array
            resultArray = new byte[byteArray.Length];
            Array.Copy(byteArray, resultArray, byteArray.Length);
        }

        int bitNumber = bitIndex % 8;
        byte mask = (byte)(1 << bitNumber);

        if (value)
            resultArray[byteIndex] |= mask; // Set the bit to 1
        else
            resultArray[byteIndex] &= (byte)~mask; // Set the bit to 0

        return resultArray;
    }
    
    public static byte[] OverwriteByteArrays(byte[] targetArray, byte[] overwriteArray)
    {
        int targetLength = targetArray.Length;
        int overwriteLength = overwriteArray.Length;

        int maxLength = Math.Max(targetLength, overwriteLength);

        byte[] resultArray = new byte[maxLength];

        for (int i = 0; i < maxLength; i++)
        {
            byte targetByte = i < targetLength ? targetArray[i] : (byte)0;
            byte overwriteByte = i < overwriteLength ? overwriteArray[i] : (byte)0;

            for (int j = 0; j < 8; j++)
            {
                bool overwriteBit = (overwriteByte & (1 << j)) != 0;
                if (i < targetLength)
                {
                    bool targetBit = (targetByte & (1 << j)) != 0;
                    if (overwriteBit)
                    {
                        targetByte = targetBit ? (byte)(targetByte | (1 << j)) : (byte)(targetByte & ~(1 << j));
                    }
                }
                else if (overwriteBit)
                {
                    targetByte |= (byte)(1 << j);
                }
            }

            resultArray[i] = targetByte;
        }

        return resultArray;
    }
}