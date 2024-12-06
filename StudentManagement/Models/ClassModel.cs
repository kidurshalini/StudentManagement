using System.Diagnostics;

namespace StudentManagement.Models
{
    public class ClassModel
    {
        public Guid Id { set; get; }

        public string Class { get; set; }

        public Guid GradeId { get; set; }
     
        public GradeModel Grades { get; set; }

       
    }
}
