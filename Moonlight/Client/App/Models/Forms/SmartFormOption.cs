using System.Linq.Expressions;
using MoonCore.Blazor.Helpers;

namespace Moonlight.Client.App.Models.Forms;

public class SmartFormOption<TForm>
{
    public readonly List<SmartFormPageConfiguration<TForm>> Pages = new();
    
    public SmartFormPageConfiguration<TForm> DefaultPage
    {
        get
        {
            var foundPage = Pages.FirstOrDefault(x => x.Name == "");

            if (foundPage == null)
            {
                foundPage = new()
                {
                    Name = ""
                };
                
                Pages.Add(foundPage);
            }

            return foundPage;
        }
    }

    public SmartFormPageConfiguration<TForm> WithPage(string name)
    {
        var foundPage = Pages.FirstOrDefault(x => x.Name == name);

        if (foundPage == null)
        {
            foundPage = new()
            {
                Name = name
            };
                
            Pages.Add(foundPage);
        }

        return foundPage;
    }
}