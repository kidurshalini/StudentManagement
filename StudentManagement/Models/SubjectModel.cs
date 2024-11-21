namespace StudentManagement.Models
{
    public class SubjectModel
    {
        public Guid ID { set; get; }

        public string Subject { get; set; }

        public Guid GradeId { get; set; }

        public GradeModel Grades { get; set; }

      
    }
}
