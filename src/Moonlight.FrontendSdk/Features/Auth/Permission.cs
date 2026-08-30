namespace Moonlight.FrontendSdk.Features.Auth;

public class Permission
{
    public string Identifier { get; set; }
    public string Group { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public Type Icon { get; set; }
}