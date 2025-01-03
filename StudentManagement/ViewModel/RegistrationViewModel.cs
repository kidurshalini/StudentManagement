﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace StudentManagement.ViewModel
{
    public class RegistrationViewModel
    {
        [Required]
        public Guid ID { get; set; }


        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        
      
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public string gender { get; set; }

        [Display(Name = "PhoneNumber")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be exactly 10 digits.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
        [DataType(DataType.PhoneNumber)]
        [ProtectedPersonalData]
        public virtual string? PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        public int Age => CalculateAge(DateOfBirth);

        private int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            int age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age)) age--;

            return age;
        }

        public string GardianName { get; set; }


        [Display(Name = "Grade")]
        public Guid GradeId { get; set; }

        [Display(Name = "Class")]
        public Guid ClassId { get; set; }

        public List<SelectListItem> Grades { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Classes { get; set; } = new List<SelectListItem>();

		public int AgeView { get; set; }
	}
}
