using System.ComponentModel.DataAnnotations;

namespace StudentManagement.ViewModel
{
    public class ProfileViewModel
    {


        public string UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }

        [DataType(DataType.Date)]
        public string BirthOfDate { get; set; }
        public string Age { get; set; }

        public string Address { get; set; }
        public string Guardian { get; set; }
        public string Grade { get; set; }
        public string Class { get; set; }
        public string Phonenumber { get; set; }
        public string Gender { get; set; }
    }
}
