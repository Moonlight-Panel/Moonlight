namespace Moonlight.Client.App.Models.Forms;

public class SmartFormPageConfiguration<TForm>
{
    public string Name { get; set; }

    public List<SmartFormSectionConfiguration<TForm>> Sections { get; set; } = new();

    public SmartFormSectionConfiguration<TForm> DefaultSection
    {
        get
        {
            var foundSection = Sections.FirstOrDefault(x => x.Name == "");

            if (foundSection == null)
            {
                foundSection = new()
                {
                    Name = ""
                };
                
                Sections.Add(foundSection);
            }

            return foundSection;
        }
    }

    public SmartFormSectionConfiguration<TForm> WithSection(string name, string? title = null, string? description = null)
    {
        var foundSection = Sections.FirstOrDefault(x => x.Name == name);

        if (foundSection == null)
        {
            foundSection = new()
            {
                Name = name,
                Title = title,
                Description = description
            };
                
            Sections.Add(foundSection);
        }

        return foundSection;
    }
}