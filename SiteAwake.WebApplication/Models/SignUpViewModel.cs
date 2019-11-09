using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SiteAwake.WebApplication.Models
{
    public class SignUpViewModel
    {        
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Url { get; set; }

        [Required]
        [Range(1, 60, ErrorMessage = "Interval must be from 1 to 60 minutes")]
        public short WakeUpIntervalMinutes { get; set; }

        public string ErrorMessage { get; set; }

        public string StatusMessage { get; set; }
    }
}