using Microsoft.AspNetCore.Identity;

namespace StudentManagement.Models
{
    public class RegistrationModel : IdentityUser
    {
        public string FullName { get; set; }

        public string Address { get; set; }

        public string GardianName { get; set; }

        public string gender { get; set; }

        public DateTime DateOfBirth { get; set; }

     

        public int Age { get; set; }

      
    }
}
