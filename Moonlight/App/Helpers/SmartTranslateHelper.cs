namespace Moonlight.App.Helpers;

public class SmartTranslateHelper
{
    private readonly Dictionary<string, Dictionary<string, string>> Languages;

    public SmartTranslateHelper()
    {
        Languages = new();
        
        foreach (var file in Directory.GetFiles("resources/lang"))
        {
            if (Path.GetExtension(file) == ".lang")
            {
                var langKey = Path.GetFileName(file)
                    .Replace(Path.GetExtension(file), "");

                var lines = File.ReadAllLines(file);
                var content = new Dictionary<string, string>();

                foreach (var line in lines)
                {
                    var parts = line.Split(";");
                    
                    if(!content.ContainsKey(parts[0]))
                        content.Add(parts[0], parts[1]);
                }
                
                Languages.Add(langKey, content);
            }
        }
    }

    public string Translate(string langKey, string content)
    {
        if (!Languages.ContainsKey(langKey))
            Languages.Add(langKey, new Dictionary<string, string>());

        if (!Languages[langKey].ContainsKey(content))
        {
            Languages[langKey].Add(content, content);
            
            File.WriteAllLines($"resources/lang/{langKey}.lang", GenerateData(Languages[langKey]));
        }

        return Languages[langKey][content];
    }

    private string[] GenerateData(Dictionary<string, string> data)
    {
        var dataList = new List<string>();

        foreach (var d in data)
            dataList.Add($"{d.Key};{d.Value}");

        return dataList.ToArray();
    }
}