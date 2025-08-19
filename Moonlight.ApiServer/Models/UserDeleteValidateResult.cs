namespace Moonlight.ApiServer.Models;

public class UserDeleteValidationResult
{
    public bool IsAllowed { get; set; }
    public string Reason { get; set; }

    public static UserDeleteValidationResult Allow()
    {
        return new UserDeleteValidationResult()
        {
            IsAllowed = true
        };
    }

    public static UserDeleteValidationResult Deny()
        => Deny("No reason provided");

    public static UserDeleteValidationResult Deny(string reason)
    {
        return new UserDeleteValidationResult()
        {
            IsAllowed = false,
            Reason = reason
        };
    }
}