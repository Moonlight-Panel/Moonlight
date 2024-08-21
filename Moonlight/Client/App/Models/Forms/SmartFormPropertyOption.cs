using System.Reflection;
using Moonlight.Client.App.UI.Components.Forms;

namespace Moonlight.Client.App.Models.Forms;

public class SmartFormPropertyOption<TForm, TProperty> : ISmartFormPropertyOption
{
    public PropertyInfo PropertyInfo { get; set; }
    public Type? ComponentType { get; set; }
    public object? OnConfigureFunc { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? Columns { get; set; }
    public object? DefaultValue { get; set; }

    public SmartFormPropertyOption<TForm, TProperty> WithName(string name)
    {
        Name = name;
        return this;
    }
    
    public SmartFormPropertyOption<TForm, TProperty> WithDescription(string description)
    {
        Description = description;
        return this;
    }
    
    public SmartFormPropertyOption<TForm, TProperty> WithColumns(int columns)
    {
        Columns = columns;
        return this;
    }

    public SmartFormPropertyOption<TForm, TProperty> WithComponent<TComponent>(Action<TComponent>? onConfigure = null) where TComponent : BaseSmartFormComponent<TProperty>
    {
        ComponentType = typeof(TComponent);
        OnConfigureFunc = onConfigure;

        return this;
    }

    public SmartFormPropertyOption<TForm, TProperty> WithDefaultValue(object value)
    {
        DefaultValue = value;
        return this;
    }
}