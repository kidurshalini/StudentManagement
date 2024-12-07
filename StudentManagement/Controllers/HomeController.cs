using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;
using System.Diagnostics;
using Newtonsoft.Json;
using StudentManagement.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace StudentManagement.Controllers
{
    public class HomeController : Controller
    {
      
    
            private readonly UserManager<RegistrationModel> _userManager;
            private readonly SignInManager<RegistrationModel> _signInManager;
            private readonly RoleManager<IdentityRole> _roleManager;
            private readonly ILogger<AccountController> _logger;
            private readonly ApplicationDbContext _context;

            public HomeController(
             UserManager<RegistrationModel> userManager,
             SignInManager<RegistrationModel> signInManager,
             RoleManager<IdentityRole> roleManager,
             ILogger<AccountController> logger,
             ApplicationDbContext context)
            {
                _userManager = userManager;
                _signInManager = signInManager;
                _roleManager = roleManager;
                _logger = logger;
                _context = context;
            }
           

        public async Task<IActionResult> GetTotalStudents()
        {
            var studentRole = "Student"; // The role name for students
            var students = await _userManager.GetUsersInRoleAsync(studentRole);
            var totalStudents = students.Count;
            ViewBag.TotalStudents = totalStudents;
            return View();
        }

        public async Task<IActionResult> GetTotalTeachers()
        {
            var teacherRole = "Teacher"; 
            var teacher = await _userManager.GetUsersInRoleAsync(teacherRole);
            var totalTeacher = teacher.Count;
            ViewBag.TotalStudents = totalTeacher;
            return View();
        }


        public async Task<IActionResult> AdminHomeAsync()
        {
            //get student total
            var studentRoleId = _context.Roles.FirstOrDefault(r => r.Name == "Student")?.Id;

            var totalStudents = _context.UserRoles
                .Count(ur => ur.RoleId == studentRoleId);
            ViewBag.TotalStudents = totalStudents;

            //get teacher total
            var teacherRoleId = _context.Roles.FirstOrDefault(r => r.Name == "Teacher")?.Id;

            var totalTeacher = _context.UserRoles
                .Count(ur => ur.RoleId == teacherRoleId);
            ViewBag.TotalTeacher = totalTeacher;

            //get subject total
            var totalSubjects = _context.Subject.Count();
            ViewBag.TotalSubjects = totalSubjects;

            var totalClass = _context.Class.Count();
            ViewBag.TotalClasses = totalClass;

            //gert grade total
            var gradesDict = _context.Grades
                .ToDictionary(gr => gr.ID, gr => gr.Grade.ToString());

            var gradeStudentCounts = _context.UserAcadamic
          .GroupBy(cr => cr.GradeId)
             .Select(g => new
    {
        GradeName = gradesDict.ContainsKey(g.Key) ? gradesDict[g.Key] : "Unknown", // Assuming gradesDict is a dictionary mapping GradeId to GradeName
        StudentCount = g.Count()
    })
    .ToList();

            // Pass serialized JSON string to ViewBag
            ViewBag.GradeData = JsonConvert.SerializeObject(gradeStudentCounts);

            //contactVIew
            var contactMessages = await _context.contactModel
           .Join(
               _context.Users, // Join with Users table
               contact => contact.UserID,
               user => user.Id,
               (contact, user) => new
               {
                   Id = contact.Id,
                   UserName = user.FullName, // Adjust according to your user model
                   UserEmail = user.Email,  // Optional
                   Message = contact.Message,
                   CreatedAt = contact.CreatedAt // Assuming a CreatedAt timestamp
               })
           .OrderByDescending(c => c.CreatedAt) // Sort by latest messages
           .ToListAsync();

            ViewBag.ContactNotifications = contactMessages;


            var userId = HttpContext.Session.GetString("UserId");

            if (!string.IsNullOrEmpty(userId))
            {
                // Fetch user's latest marks based on the most recent term
                var latestMarks = _context.MarksDetail
                    .Where(m => m.UserID == userId)
                    .OrderByDescending(m => m.CreatedAt) // Assuming CreatedAt is a DateTime property in MarksDetail
                    .GroupBy(m => m.Term) // Group by term to fetch the latest term's marks
                    .Select(g => new
                    {
                        Term = g.Key,
                        Marks = g.Join(_context.Subject, // Join with Subject table
                                       m => m.SubjectID, // Foreign key in Marks table
                                       s => s.ID,        // Primary key in Subject table
                                       (m, s) => new { s.Subject, m.Marks }).ToList(),
                        LastUpdated = g.Max(m => m.CreatedAt) // Get the most recent update time for the term
                    })
                    .OrderByDescending(g => g.LastUpdated) // Ensure the latest term comes first
                    .FirstOrDefault(); // Fetch the latest term's data

                if (latestMarks != null)
                {
                    ViewBag.Term = latestMarks.Term;
                    ViewBag.UserMarks = latestMarks.Marks;
                    ViewBag.LastUpdated = latestMarks.LastUpdated;
                    ViewBag.AlertMessage = $"Your {latestMarks.Term} marks are now available!";
                }
                else
                {
                    ViewBag.AlertMessage = "No marks found for this user.";
                }
            }
            else
            {
                ViewBag.AlertMessage = "User not found in the session.";
            }

            return View();
        }

        public IActionResult SubmitFeedback(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // If the validation fails, return the same view with the model and errors
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                return RedirectToAction("AdminHome");
            }


            var userId = HttpContext.Session.GetString("UserId");
            var email = HttpContext.Session.GetString("UserEmail");

            var newfeedback = new contactModel
            {
                UserID = userId,
                Message = model.Message

            };

            _context.contactModel.Add(newfeedback);
            _context.SaveChanges();


            TempData["SuccessMessage"] = "Thanks for Valueble feedback and conserns!";
            return RedirectToAction("AdminHome");

        }

       

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
