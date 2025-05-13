using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Scripts.Helpers;

public class StartupClassDetector
{
    public bool Check(string content, out string fullName)
    {
        var tree = CSharpSyntaxTree.ParseText(content);
        var root = tree.GetCompilationUnitRoot();
        
        var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        var compilation = CSharpCompilation.Create("MyAnalysis", [tree], [mscorlib]);

        var model = compilation.GetSemanticModel(tree);

        var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

        foreach (var classDecl in classDeclarations)
        {
            var symbol = model.GetDeclaredSymbol(classDecl);
            
            if(symbol == null)
                continue;
            
            var hasAttribute = symbol.GetAttributes().Any(attribute =>
            {
                if (attribute.AttributeClass == null)
                    return false;

                return attribute.AttributeClass.Name.Contains("PluginStartup");
            });

            if (hasAttribute)
            {
                fullName = symbol.ContainingNamespace.ToDisplayString() + "." + classDecl.Identifier.ValueText;
                return true;
            }
        }

        fullName = "";
        return false;
    }
}