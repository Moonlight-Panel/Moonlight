using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using MoonCore.Blazor.Helpers;

namespace Moonlight.Client.App.Models.Forms;

public class SmartFormSectionConfiguration<TForm>
{
    public string Name { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }

    public readonly List<ISmartFormItem> Items = new();
    
    public SmartFormPropertyOption<TForm, TProperty> AddProperty<TProperty>(Expression<Func<TForm, TProperty>> func)
    {
        var option = new SmartFormPropertyOption<TForm, TProperty>();

        option.PropertyInfo = FormHelper.GetPropertyInfo(func);
        
        Items.Add(option);
        
        return option;
    }
    
    public ISmartFormPropertyOption AddProperty(PropertyInfo propertyInfo)
    {
        var typeToCreate = typeof(SmartFormPropertyOption<,>).MakeGenericType(typeof(TForm), propertyInfo.PropertyType);
        var option = (Activator.CreateInstance(typeToCreate) as ISmartFormPropertyOption)!;

        option.PropertyInfo = propertyInfo;
        
        Items.Add(option);
        
        return option;
    }

    public void AddComponent<T>(int columns = 6, Action<Dictionary<string, object>>? parameters = null) where T : ComponentBase
    {
        Items.Add(new SmartFormComponent()
        {
            Render = ComponentHelper.FromType<T>(buildAttributes: parameters),
            Columns = 6
        });
    }
}