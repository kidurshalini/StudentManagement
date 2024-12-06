using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Models
{
    public class contactModel

    {
        public Guid Id { get; set; }

        // Foreign Key to RegistrationModel (User)
        public string UserID { get; set; }
        public RegistrationModel Registration { get; set; }

        [Required]
        public string Message { get; set; }



        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
