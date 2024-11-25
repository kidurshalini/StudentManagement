using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace StudentManagement.Models
{
    public class GradeModel
    {
        public Guid ID { set; get; }

        public int Grade { get; set; }

        public ICollection<ClassModel> Class { get; set; }
        public ICollection<SubjectModel> Subject { get; set; }
        
    }
}
