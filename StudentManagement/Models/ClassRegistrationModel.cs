namespace StudentManagement.Models
{
	public class ClassRegistrationModel
	{
        public Guid Id { get; set; }

        // Foreign Key to RegistrationModel (User)
        public string UserID { get; set; }
        public RegistrationModel Registration { get; set; }

        // Foreign Key to Grade
        public Guid GradeId { get; set; }
        public GradeModel Grades { get; set; }

        // Foreign Key to Class
        public Guid ClassId { get; set; }
        public ClassModel Class { get; set; }

        // Collection of users associated with this class registration (can be one-to-many)
        public ICollection<RegistrationModel> Users { get; set; }

        // Collection of classes associated with this registration (can be one-to-many)
        public ICollection<ClassModel> Classes { get; set; }

        // Subjects related to the class
        public ICollection<SubjectModel> Subject { get; set; }
    }
}
