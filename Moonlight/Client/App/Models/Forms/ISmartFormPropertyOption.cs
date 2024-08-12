using System.Reflection;

namespace Moonlight.Client.App.Models.Forms;

public interface ISmartFormPropertyOption
{
    //public List<IFastFormValidator> Validators { get; set; }
    //public FastFormPageConfiguration? PageConfiguration { get; set; }
    //public FastFormSectionConfiguration? SectionConfiguration { get; set; }
    public PropertyInfo PropertyInfo { get; set; }
    public Type? ComponentType { get; set; }
    public object? OnConfigureFunc { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Columns { get; set; }
}