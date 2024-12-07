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



        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<SelectListItem> Grades { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Classes { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Users { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Subjects { get; set; } = new List<SelectListItem>();
        public Dictionary<Guid, string> MarksDetail { get; set; }



        [Range(0, 100, ErrorMessage = "Marks must be between 0 and 100.")]
        public string Marks { get; set; }


        public string Term { get; set; }

        public GradeModel Grade { get; set; }
        public ClassModel Class { get; set; }

        public Dictionary<Guid, string> SubjectMarks { get; set; }

    }
}
