using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

//Done: Make this namespace match your project name
namespace ChenKennethHW5.Models
{
    public class AppUser : IdentityUser
    {
        //Done: Add custom user fields - first name is included as an example
        [Display(Name = "First Name")]
        public String FirstName { get; set; }

        [Display(Name = "Last Name")]
        public String LastName { get; set; }

        //Navigational Properties: One customer can have many orders
        public List<Order>? Orders { get; set; }

    }
}
