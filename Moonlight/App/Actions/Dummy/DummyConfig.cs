using System.ComponentModel;
using Moonlight.App.Database.Entities;
using Moonlight.App.Extensions.Attributes;

namespace Moonlight.App.Actions.Dummy;

public class DummyConfig
{
    [Description("Some description")]
    public string String { get; set; } = "";
    public bool Boolean { get; set; }
    public int Integer { get; set; }
}