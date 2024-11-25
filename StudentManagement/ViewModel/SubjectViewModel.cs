using Microsoft.AspNetCore.Mvc.Rendering;
using StudentManagement.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace StudentManagement.ViewModel
{
    public class SubjectViewModel
    {
        public List<SubjectModel> Subjects;

        public Guid Id { get; set; }

		[Required(ErrorMessage = "Grade is required.")]
        [Display(Name = "Grade")]
        public Guid GradeId { get; set; }

        [Required(ErrorMessage = "Subject name is required.")]
        [Display(Name = "Subject Name")]
        public string Subject { get; set; } // Subject Name

       public List<SelectListItem> Grades { get; set; } = new List<SelectListItem>();

	 
      
    }
}
