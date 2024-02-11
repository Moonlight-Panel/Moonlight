﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Moonlight.Features.Community.Models.Forms.Admin;

public class EditWordFilter
{
    [Required(ErrorMessage = "You need to specify a filter")]
    [Description(
        "This filters all posts and comments created using this regex. If any match is found it will block the action")]
    public string Filter { get; set; } = "";
}