using System.Globalization;

namespace Moonlight.FrontendSdk.Helpers;

public static class Formatter
{
    public static string TrimWithDots(string input, int maxLength)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input;

        if (maxLength <= 3)
            return input.Substring(0, Math.Min(input.Length, maxLength));

        return string.Concat(input.AsSpan(0, maxLength - 3), "...");
    }

    public static string FormatDateTimeOffset(DateTimeOffset dateTimeOffset)
        => dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}