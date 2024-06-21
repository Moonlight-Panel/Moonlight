namespace Moonlight.Core.Attributes;

public class ApiDocumentAttribute : Attribute
{
    public string Name { get; set; }
    
    public ApiDocumentAttribute(string name)
    {
        Name = name;
    }
}