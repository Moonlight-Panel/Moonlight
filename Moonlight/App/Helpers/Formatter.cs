using System.Text;
using Microsoft.AspNetCore.Components;

namespace Moonlight.App.Helpers;

public static class Formatter
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

    public static bool EndsInOneOf(string suffix, IEnumerable<string> strings)
    {
        foreach (string str in strings)
        {
            if (suffix.EndsWith(str))
            {
                return true;
            }
        }

        return false;
    }

    public static bool ContainsOneOf(string textToSearch, IEnumerable<string> strings, out string foundText)
    {
        foreach (string str in strings)
        {
            if (textToSearch.Contains(str))
            {
                foundText = str;
                return true;
            }
        }

        foundText = "";
        return false;
    }

    public static bool ContainsOneOf(string textToSearch, IEnumerable<string> strings)
    {
        return ContainsOneOf(textToSearch, strings, out _);
    }

    public static string FormatSize(long bytes)
    {
        var i = Math.Abs(bytes) / 1024D;
        if (i < 1)
        {
            return bytes + " B";
        }
        else if (i / 1024D < 1)
        {
            return i.Round(2) + " KB";
        }
        else if (i / (1024D * 1024D) < 1)
        {
            return (i / 1024D).Round(2) + " MB";
        }
        else
        {
            return (i / (1024D * 1024D)).Round(2) + " GB";
        }
    }

    private static double Round(this double d, int decimals)
    {
        return Math.Round(d, decimals);
    }

    public static string ReplaceEnd(string input, string substringToReplace, string newSubstring)
    {
        int lastIndexOfSubstring = input.LastIndexOf(substringToReplace);
        if (lastIndexOfSubstring >= 0)
        {
            input = input.Remove(lastIndexOfSubstring, substringToReplace.Length)
                .Insert(lastIndexOfSubstring, newSubstring);
        }

        return input;
    }

    public static string ConvertCamelCaseToSpaces(string input)
    {
        StringBuilder output = new StringBuilder();

        foreach (char c in input)
        {
            if (char.IsUpper(c))
            {
                output.Append(' ');
            }

            output.Append(c);
        }

        return output.ToString().Trim();
    }

    public static string FormatUptime(double uptime)
    {
        TimeSpan t = TimeSpan.FromMilliseconds(uptime);

        return FormatUptime(t);
    }

    public static string FormatUptime(TimeSpan t)
    {
        if (t.Days > 0)
        {
            return $"{t.Days}d  {t.Hours}h {t.Minutes}m {t.Seconds}s";
        }
        else
        {
            return $"{t.Hours}h {t.Minutes}m {t.Seconds}s";
        }
    }

    public static string FormatDate(DateTime e)
    {
        string i2s(int i)
        {
            if (i.ToString().Length < 2)
                return "0" + i;
            return i.ToString();
        }

        return $"{i2s(e.Day)}.{i2s(e.Month)}.{e.Year} {i2s(e.Hour)}:{i2s(e.Minute)}";
    }

    public static string FormatDateOnly(DateTime e)
    {
        string i2s(int i)
        {
            if (i.ToString().Length < 2)
                return "0" + i;
            return i.ToString();
        }

        return $"{i2s(e.Day)}.{i2s(e.Month)}.{e.Year}";
    }

    public static string FormatSize(double bytes)
    {
        var i = Math.Abs(bytes) / 1024D;
        if (i < 1)
        {
            return bytes + " B";
        }
        else if (i / 1024D < 1)
        {
            return i.Round(2) + " KB";
        }
        else if (i / (1024D * 1024D) < 1)
        {
            return (i / 1024D).Round(2) + " MB";
        }
        else
        {
            return (i / (1024D * 1024D)).Round(2) + " GB";
        }
    }

    public static RenderFragment FormatLineBreaks(string content)
    {
        return builder =>
        {
            int i = 0;
            var arr = content.Split("\n");

            foreach (var line in arr)
            {
                builder.AddContent(i, line);
                if (i++ != arr.Length - 1)
                {
                    builder.AddMarkupContent(i, "<br/>");
                }
            }
        };
    }

    // This will replace every placeholder with the respective value if specified in the model
    // For example:
    // A instance of the user model has been passed in the 'models' parameter of the function.
    // So the placeholder {{User.Email}} will be replaced by the value of the Email property of the model
    public static string ProcessTemplating(string text, params object[] models)
    {
        foreach (var model in models)
        {
            foreach (var property in model.GetType().GetProperties())
            {
                var value = property.GetValue(model);
                
                if(value == null)
                    continue;

                var placeholder = "{{" + $"{model.GetType().Name}.{property.Name}" + "}}";

                text = text.Replace(placeholder, value.ToString());
            }
        }

        return text;
    }
}