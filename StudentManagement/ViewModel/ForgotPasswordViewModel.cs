using System.ComponentModel.DataAnnotations;

namespace StudentManagement.ViewModel
{
    public class ForgotPasswordViewModel
    {
       
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        

    }
}
