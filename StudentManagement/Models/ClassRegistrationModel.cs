namespace StudentManagement.Models
{
	public class ClassRegistrationModel
	{
		public Guid Id { get; set; }

        public string UserID { get; set; }

        public RegistrationModel Registration { get; set; }

        public Guid GradeId { get; set; }

        public GradeModel Grades { get; set; }

        public Guid ClassId { get; set; }

        public ClassModel Class { get; set; }

        public ICollection<RegistrationModel> Users { get; set; }

        public ICollection<ClassModel> Classes { get; set; }
        public ICollection<SubjectModel> Subject { get; set; }

    }
}
