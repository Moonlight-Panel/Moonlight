using System.ComponentModel.DataAnnotations;

namespace Moonlight.App.Models.Forms.Community;

public class AddPostForm
{
    [Required(ErrorMessage = "You need to enter a title")]
    [MaxLength(40, ErrorMessage = "The title can only be 40 characters long")]
    [MinLength(8, ErrorMessage = "The title must at least have 8 characters")]
    public string Title { get; set; } = "";
    
    [Required(ErrorMessage = "You need to enter post content")]
    [MaxLength(2048, ErrorMessage = "The post content can only be 2048 characters long")]
    [MinLength(8, ErrorMessage = "The post content must at least have 8 characters")]
    public string Content { get; set; } = "";
}