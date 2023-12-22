﻿namespace Moonlight.App.Models.Abstractions;

public class ApplicationTheme
{
    public string Name { get; set; } = "";
    public string Author { get; set; } = "";
    public string? DonateUrl { get; set; } = "";
    public string CssUrl { get; set; } = "";
    public string? JsUrl { get; set; } = "";

    public bool Enabled { get; set; } = false;
}