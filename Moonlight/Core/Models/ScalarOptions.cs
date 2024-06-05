﻿namespace Moonlight.Core.Models;

// From https://github.com/scalar/scalar/blob/main/packages/scalar.aspnetcore/ScalarOptions.cs

public class ScalarOptions
{
    public string Theme { get; set; } = "purple";

    public bool? DarkMode { get; set; }
    public bool? HideDownloadButton { get; set; }
    public bool? ShowSideBar { get; set; }

    public bool? WithDefaultFonts { get; set; }

    public string? Layout { get; set; }

    public string? CustomCss { get; set; }

    public string? SearchHotkey { get; set; }

    public Dictionary<string, string>? Metadata { get; set; }

    public ScalarAuthenticationOptions? Authentication { get; set; }
}

public class ScalarAuthenticationOptions
{
    public string? PreferredSecurityScheme { get; set; }

    public ScalarAuthenticationApiKey? ApiKey { get; set; }
}

public class ScalarAuthenticationoAuth2
{
    public string? ClientId { get; set; }

    public List<string>? Scopes { get; set; }
}

public class ScalarAuthenticationApiKey
{
    public string? Token { get; set; }
}