using System.Text;

namespace Moonlight.App.Helpers;

public static class StringHelper
{
    public static string GenerateString(int length)
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringBuilder = new StringBuilder();
        var random = new Random();

        for (int i = 0; i < length; i++)
        {
            stringBuilder.Append(chars[random.Next(chars.Length)]);
        }

        return stringBuilder.ToString();
    }

    public static string IntToStringWithLeadingZeros(int number, int n)
    {
        string result = number.ToString();
        int length = result.Length;

        for (int i = length; i < n; i++)
        {
            result = "0" + result;
        }

        return result;
    }
    
    public static string CapitalizeFirstCharacter(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        char firstChar = char.ToUpper(input[0]);
        string restOfString = input.Substring(1);

        return firstChar + restOfString;
    }

    public static string CutInHalf(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        int length = input.Length;
        int halfLength = length / 2;

        return input.Substring(0, halfLength);
    }
}