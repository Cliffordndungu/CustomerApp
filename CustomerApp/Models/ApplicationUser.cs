using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CustomerApp.Models
{
    public class ApplicationUser: IdentityUser
    {
       

      
        [Display(Name = "Full name")]
        public string Fullname { get; set; }

        [Display(Name = "Company name")]
        public string Company { get; set; }

        [Display(Name = "Phone number")]
        public string Phonenumber { get; set; }
    }
}
