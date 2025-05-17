using Cocona;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoonCore.Extensions;
using Scripts.Commands;
using Scripts.Helpers;

Console.WriteLine("Moonlight Build Helper Script");
Console.WriteLine();

var builder = CoconaApp.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddMoonCore();

builder.Services.AddSingleton<CommandHelper>();
builder.Services.AddSingleton<NupkgHelper>();
builder.Services.AddSingleton<CsprojHelper>();
builder.Services.AddSingleton<CodeHelper>();

var app = builder.Build();

app.AddCommands<PackCommand>();
app.AddCommands<PreBuildCommand>();

await app.RunAsync();