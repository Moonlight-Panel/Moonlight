using System.Linq.Expressions;
using MoonCore.Blazor.Helpers;

namespace Moonlight.Client.App.Models.Forms;

public class SmartFormSectionConfiguration<TForm>
{
    public string Name { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }

    public readonly List<ISmartFormPropertyOption> Properties = new();
    
    public SmartFormPropertyOption<TForm, TProperty> AddProperty<TProperty>(Expression<Func<TForm, TProperty>> func)
    {
        var option = new SmartFormPropertyOption<TForm, TProperty>();

        option.PropertyInfo = FormHelper.GetPropertyInfo(func);
        
        Properties.Add(option);
        
        return option;
    }
}