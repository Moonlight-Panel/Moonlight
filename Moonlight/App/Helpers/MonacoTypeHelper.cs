namespace Moonlight.App.Helpers;

public class MonacoTypeHelper
{
    public static string GetEditorType(string file)
    {
        var extension = Path.GetExtension(file);
        extension = extension.TrimStart("."[0]);

        switch (extension)
        {
            case "bat":
                return "bat";
            case "cs":
                return "csharp";
            case "css":
                return "css";
            case "html":
                return "html";
            case "java":
                return "java";
            case "js":
                return "javascript";
            case "ini":
                return "ini";
            case "json":
                return "json";
            case "lua":
                return "lua";
            case "php":
                return "php";
            case "py":
                return "python";
            case "sh":
                return "shell";
            case "xml":
                return "xml";
            case "yml":
                return "yaml";
            default:
                return "plaintext";
        }
    }
}