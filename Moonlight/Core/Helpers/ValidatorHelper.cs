using System.ComponentModel.DataAnnotations;
using Moonlight.Core.Exceptions;

namespace Moonlight.Core.Helpers;

public class ValidatorHelper
{
    public static Task Validate(object objectToValidate)
    {
        var context = new ValidationContext(objectToValidate, null, null);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(objectToValidate, context, results, true);

        if (!isValid)
        {
            var errorMsg = "Unknown form error";

            if (results.Any())
                errorMsg = results.First().ErrorMessage ?? errorMsg;

            throw new DisplayException(errorMsg);
        }
        
        return Task.CompletedTask;
    }
    
    public static async Task ValidateRange(IEnumerable<object> objectToValidate)
    {
        foreach (var o in objectToValidate)
            await Validate(o);
    }
}