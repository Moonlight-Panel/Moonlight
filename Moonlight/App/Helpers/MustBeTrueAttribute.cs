using System.ComponentModel.DataAnnotations;
using Logging.Net;

namespace Moonlight.App.Helpers;

[AttributeUsage(AttributeTargets.Property)]
public class MustBeTrueAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is bool boolValue)
        {
            return boolValue;
        }

        return false;
    }
}