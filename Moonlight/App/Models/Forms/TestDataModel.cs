using System.ComponentModel.DataAnnotations;
using Moonlight.App.Database.Entities;

namespace Moonlight.App.Models.Forms;

public class TestDataModel
{
    [Required]
    public User User { get; set; }
}