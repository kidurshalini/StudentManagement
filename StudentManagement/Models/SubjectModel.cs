namespace StudentManagement.Models
{
    public class SubjectModel
    {
        public Guid ID { set; get; }

        public int Subject { get; set; }

        public Guid GradeId { get; set; }

        public GradeModel Grade { get; set; }
    }
}
