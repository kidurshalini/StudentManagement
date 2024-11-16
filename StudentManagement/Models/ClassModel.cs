using System.Diagnostics;

namespace StudentManagement.Models
{
    public class ClassModel
    {
        public Guid ID { set; get; }

        public int Class { get; set; }

        public Guid GradeId { get; set; }
     
        public GradeModel Grade { get; set; }

  

    }
}
