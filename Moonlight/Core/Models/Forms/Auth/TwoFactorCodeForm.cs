﻿using System.ComponentModel.DataAnnotations;

namespace Moonlight.Core.Models.Forms.Auth;

public class TwoFactorCodeForm
{
    [Required(ErrorMessage = "You need to enter a two factor code")]
    public string Code { get; set; } = "";
}