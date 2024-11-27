using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using System.Security.Claims;

namespace StudentManagement.Models
{
    public class RegistrationModel : IdentityUser
    {
        public string FullName { get; set; }

        public string Address { get; set; }

        public string? GardianName { get; set; }

        public string gender { get; set; }

        public DateTime DateOfBirth { get; set; }


        public int Age { get; set;


}
    }
}