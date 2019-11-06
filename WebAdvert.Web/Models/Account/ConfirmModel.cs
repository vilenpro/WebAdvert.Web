﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAdvert.Web.Models.Account
{
    public class ConfirmModel
    {
        [Required]
        [EmailAddress]
        [Display(Name ="Email Address")]
        public string Email { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
