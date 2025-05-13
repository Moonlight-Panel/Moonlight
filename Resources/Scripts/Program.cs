using Cocona;
using Microsoft.Extensions.DependencyInjection;
using Scripts.Commands;
using Scripts.Helpers;

Console.WriteLine("Moonlight Build Helper Script");
Console.WriteLine();

var builder = CoconaApp.CreateBuilder(args);

builder.Services.AddSingleton<CommandHelper>();
builder.Services.AddSingleton<StartupClassDetector>();

var app = builder.Build();

app.AddCommands<PackCommand>();
app.AddCommands<PreBuildCommand>();

await app.RunAsync();