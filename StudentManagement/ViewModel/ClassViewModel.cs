using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentManagement.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace StudentManagement.ViewModel
{
	public class ClassViewModel

    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Grade is required.")]
        [Display(Name = "Grade")]
        public Guid GradeId { get; set; }

        [Required(ErrorMessage = "Class  is required.")]
        [Display(Name = "Subject Name")]
        public string Class { get; set; }

        public List<SelectListItem> Grades { get; set; } = new List<SelectListItem>();
    }

}