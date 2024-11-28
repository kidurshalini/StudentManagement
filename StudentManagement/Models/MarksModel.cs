namespace StudentManagement.Models
{
    public class MarksModel
    {

        public Guid Id { get; set; }

        // Foreign Key to RegistrationModel (User)
        public string UserID { get; set; }
        public RegistrationModel Registration { get; set; }

        // Foreign Key to Grade
        public Guid SubjectID { get; set; }
        public SubjectModel Subject { get; set; }
       
        public string Marks { get; set; }

        public string Term { get; set; }
    }
}
