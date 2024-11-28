using Microsoft.AspNetCore.Mvc.Rendering;
using StudentManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace StudentManagement.ViewModel
{
    public class MarksViewModel
    {

        public Guid Id { get; set; }

        // Foreign Key to RegistrationModel (User)
        public string UserID { get; set; }

        public RegistrationModel Registration { get; set; }

        // Foreign Key to Grade
        public Guid SubjectID { get; set; }
        public SubjectModel Subject { get; set; }

        [Required(ErrorMessage = "Grade is required.")]
        [Display(Name = "Grade")]
        public Guid GradeId { get; set; }

        [Display(Name = "Class")]
        public Guid ClassId { get; set; }

        [Display(Name = "Subjects")]
        public List<Guid> SubjectId { get; set; } = new List<Guid>();


        public List<SelectListItem> Grades { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Classes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Users { get; set; }

        public Dictionary<Guid, string> SubjectMarks { get; set; }
        public string Marks { get; set; }
        public string Term { get; set; }
    }
}
