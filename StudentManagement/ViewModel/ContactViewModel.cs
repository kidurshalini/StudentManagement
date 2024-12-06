using StudentManagement.Models;

namespace StudentManagement.ViewModel
{
    public class ContactViewModel
    {
        public Guid Id { get; set; }

        // Foreign Key to RegistrationModel (User)
 

        public string Message { get; set; }
    }
}
