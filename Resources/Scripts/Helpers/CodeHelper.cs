using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace Scripts.Helpers;

public class CodeHelper
{
    private readonly ILogger<CodeHelper> Logger;

    public CodeHelper(ILogger<CodeHelper> logger)
    {
        Logger = logger;
    }

    public async Task<string[]> FindPluginStartups(string[] filesToSearch)
    {
        var result = new List<string>();
        
        var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

        var trees = new List<SyntaxTree>();

        foreach (var file in filesToSearch)
        {
            Logger.LogDebug("Reading {file}", file);

            var content = await File.ReadAllTextAsync(file);
            var tree = CSharpSyntaxTree.ParseText(content);
            trees.Add(tree);
        }

        var compilation = CSharpCompilation.Create("Analysis", trees, [mscorlib]);

        foreach (var tree in trees)
        {
            var model = compilation.GetSemanticModel(tree);
            var root = await tree.GetRootAsync();

            var classDeclarations = root
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>();

            foreach (var classDeclaration in classDeclarations)
            {
                var symbol = model.GetDeclaredSymbol(classDeclaration);

                if (symbol == null)
                    continue;

                var hasAttribute = symbol.GetAttributes().Any(attr =>
                {
                    if (attr.AttributeClass == null)
                        return false;

                    return attr.AttributeClass.Name == "PluginStartup";
                });

                if (!hasAttribute)
                    continue;

                var classPath = $"{symbol.ContainingNamespace.ToDisplayString()}.{classDeclaration.Identifier.ValueText}";
                
                Logger.LogInformation("Detected startup in class: {classPath}", classPath);
                
                result.Add(classPath);
            }
        }
        
        return result.ToArray();
    }
}