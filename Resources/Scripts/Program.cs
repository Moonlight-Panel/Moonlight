using Scripts.Functions;

if (args.Length == 0)
{
    Console.WriteLine("You need to specify a module to run");
    return;
}

var module = args[0];
var moduleArgs = args.Skip(1).ToArray();

switch (module)
{
    case "staticWebAssets":
        await StaticWebAssetsFunctions.Run(moduleArgs);
        break;
    case "content":
        await ContentFunctions.Run(moduleArgs);
        break;
    case "src":
        await SrcFunctions.Run(moduleArgs);
        break;
    default:
        Console.WriteLine($"No module named {module} found");
        break;
}