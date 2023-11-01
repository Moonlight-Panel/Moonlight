namespace Moonlight.App.Api;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ApiRequestAttribute : Attribute
{
    public int Id { get; set; }
    public ApiRequestAttribute(int id)
    {
        Id = id;
    }
}