using System.ComponentModel.DataAnnotations;

namespace StudentManagement.ViewModel
{
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
		public string Token { get; set; }

		[Required]
		[StringLength(100, MinimumLength = 6)]
		public string Password { get; set; }

		[Required]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }



	}
}
