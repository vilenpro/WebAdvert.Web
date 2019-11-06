using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAdvert.Web.Models.Account
{
    public class ResetPasswordModel
    {
        public string Email { get; set; }
        public string Code { get; set; }

    }
}
