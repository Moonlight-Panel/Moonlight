namespace Moonlight.App.Database.Entities.Store;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Slug { get; set; } = "";
}