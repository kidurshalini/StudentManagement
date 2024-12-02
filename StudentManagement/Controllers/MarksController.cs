using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using StudentManagement.Models;
using StudentManagement.ViewModel;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudentManagement.Controllers
{
    public class MarksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MarksController(ApplicationDbContext context)
        {
            _context = context;
        }
        private async Task PopulateDropdowns(MarksViewModel model, Guid? selectedGradeId = null)
        {
            model.Grades = await _context.Grades
                .Select(g => new SelectListItem
                {
                    Value = g.ID.ToString(),
                    Text = $"Grade {g.Grade}"
                })
                .ToListAsync();

            if (selectedGradeId.HasValue)
            {
                model.Classes = await _context.Class
                    .Where(c => c.GradeId == selectedGradeId.Value) // Filter classes by selected grade
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = $"Class {c.Class}"
                    })
                    .ToListAsync();
            }
            else
            {
                model.Classes = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- Select Class --" }
        };
            }
        }


        [HttpGet]
        public async Task<IActionResult> Marksform()
        {
            var viewModel = new MarksViewModel();
            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateClasses(Guid gradeId)
        {
            if (gradeId == Guid.Empty)
            {
                return Json(new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- No Classes Available --" }
        });
            }

            var classes = await _context.Class
                .Where(c => c.GradeId == gradeId)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"Class {c.Class}" // Ensure this matches the class property name
                })
                .ToListAsync();

            if (!classes.Any())
            {
                classes.Add(new SelectListItem { Value = "", Text = "-- No Classes Available --" });
            }

            return Json(classes);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersByGradeAndClass(Guid gradeId, Guid classId)
        {
            if (gradeId == Guid.Empty || classId == Guid.Empty)
            {
                return Json(new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- No Users Available --" }
        });
            }

            // Join AcademicDetails and Users to filter by grade and class
            var users = await _context.UserAcadamic
                .Where(ad => ad.GradeId == gradeId && ad.ClassId == classId) // Filter by Grade and Class
                .Join(_context.Users,
                    ad => ad.UserID,
                    u => u.Id,
                    (ad, u) => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = u.FullName // Assuming UserName is the property you want to display
                    })
                .ToListAsync();

            if (!users.Any())
            {
                users.Add(new SelectListItem { Value = "", Text = "-- No Users Available --" });
            }

            return Json(users);
        }
        public IActionResult SubjectGenerator(Guid gradeId)
        {
            if (gradeId == Guid.Empty)
            {
                return RedirectToAction("MarksForm");
            }

            var subjects = _context.Subject
                .Where(s => s.GradeId == gradeId)
                .Select(s => new { SubjectId = s.ID, SubjectName = s.Subject })
                .ToList();

            var htmlBuilder = new StringBuilder();

            foreach (var subject in subjects)
            {
                htmlBuilder.AppendLine("<div class='row mb-3'>");
                htmlBuilder.AppendLine($"  <label class='col-md-4 col-form-label' for='subject_{subject.SubjectId}'>{subject.SubjectName}</label>");
                htmlBuilder.AppendLine($"  <div class='col-md-4'>");
                htmlBuilder.AppendLine($"    <input type='text' class='form-control' name='SubjectMarks[{subject.SubjectId}]' id='subject_{subject.SubjectId}' placeholder='Enter marks' />");
                htmlBuilder.AppendLine($"  </div>");
                htmlBuilder.AppendLine("</div>");
            }

            return Content(htmlBuilder.ToString());
        }

        public IActionResult MarksView(MarksViewModel model)
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MarksView()
        {
            var viewModel = new MarksViewModel();
            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult MarksFormSubmit(MarksViewModel model)
        {
            if (model == null || model.SubjectMarks == null || !model.SubjectMarks.Any())
            {
                TempData["ErrorMessage"] = "No marks submitted.";
                return RedirectToAction("MarksForm");
            }

            var existingUser = _context.MarksDetail
                     .FirstOrDefault(s => s.UserID == model.UserID && s.Term == model.Term);

            if(existingUser != null)
            {
                TempData["ErrorMessage"] = "In this Term User already been entered.";

                return View("MarksForm",model);
            }


            foreach (var entry in model.SubjectMarks)
            {
                var marksDetails = new MarksModel
                {
                    Id = Guid.NewGuid(),
                    UserID = model.UserID,
                    Term = model.Term,
                    SubjectID = entry.Key,
                    Marks = entry.Value,
                    ClassId = model.ClassId,
                       GradeId = model.GradeId
                };

                _context.MarksDetail.Add(marksDetails);
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Marks submitted successfully!";
            return RedirectToAction("MarksForm");
        }

        [HttpGet]
        public async Task<IActionResult> GetTermsByGrade(Guid gradeId)
        {
            if (gradeId == Guid.Empty)
            {
                return Json(new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "-- No Terms Available --" }
                });
            }

            var terms = await _context.MarksDetail
                .Where(m => m.GradeId == gradeId)
                .Select(m => m.Term)
                .Distinct()
                .OrderBy(term => term)
                .Select(term => new SelectListItem
                {
                    Value = term.ToString(),
                    Text = term.ToString()
                })
                .ToListAsync();

            if (!terms.Any())
            {
                terms.Add(new SelectListItem { Value = "", Text = "-- No Terms Available --" });
            }

            return Json(terms);
        }


        public IActionResult MarksSheetGenerator(Guid gradeId, Guid classId, string term)
        {
            if (gradeId == Guid.Empty || classId == Guid.Empty || string.IsNullOrEmpty(term))
            {
                return RedirectToAction("MarksForm");
            }

            var subjects = _context.Subject
                .Where(s => s.GradeId == gradeId)
                .Select(s => new { s.ID, s.Subject })
                .ToList();

            var gradeName = _context.Grades
                .Where(g => g.ID == gradeId)
                .Select(g => g.Grade.ToString())
                .FirstOrDefault() ?? "Unknown Grade";

            var marks = _context.MarksDetail
                .Where(m => m.GradeId == gradeId && m.ClassId == classId && m.Term == term)
                .ToList();

            var htmlBuilder = new StringBuilder();

            htmlBuilder.AppendLine("<div class='container mt-4'>");
            htmlBuilder.AppendLine("  <div class='card shadow-sm'>");
            htmlBuilder.AppendLine("    <div class='card-header text-white bg-primary'>");
            htmlBuilder.AppendLine($"      <h4 class='mb-0 text-center'>Marksheet for Grade {gradeName} - {term}</h4>");
            htmlBuilder.AppendLine("    </div>");
            htmlBuilder.AppendLine("    <div class='card-body'>");

            htmlBuilder.AppendLine("      <div class='table-responsive'>");
            htmlBuilder.AppendLine("        <table class='table table-striped table-hover text-center table-bordered'>");
            htmlBuilder.AppendLine("          <thead>");
            htmlBuilder.AppendLine("            <tr>");
            htmlBuilder.AppendLine("              <th>Student</th>");

            foreach (var subject in subjects)
            {
                htmlBuilder.AppendLine($"              <th>{subject.Subject}</th>");
            }

            htmlBuilder.AppendLine("            </tr>");
            htmlBuilder.AppendLine("          </thead>");
            htmlBuilder.AppendLine("          <tbody>");

            var students = marks.GroupBy(m => m.UserID);

            foreach (var studentGroup in students)
            {
                var studentId = studentGroup.Key;
                var studentName = _context.Users
                    .Where(r => r.Id == studentId)
                    .Select(r => r.FullName)
                    .FirstOrDefault() ?? "Unknown Student";

                htmlBuilder.AppendLine("            <tr>");
                htmlBuilder.AppendLine($"              <td>{studentName}</td>");

                foreach (var subject in subjects)
                {
                    var studentMarks = studentGroup
                        .FirstOrDefault(m => m.SubjectID == subject.ID)?.Marks ?? "N/A";

                    htmlBuilder.AppendLine($"              <td>{studentMarks}</td>");
                }

                htmlBuilder.AppendLine("            </tr>");
            }

            htmlBuilder.AppendLine("          </tbody>");
            htmlBuilder.AppendLine("        </table>");
            htmlBuilder.AppendLine("      </div>");
            htmlBuilder.AppendLine("    </div>");
            htmlBuilder.AppendLine("  </div>");
            htmlBuilder.AppendLine("</div>");

            return Content(htmlBuilder.ToString(), "text/html");
        }



        public IActionResult MarksAllView()
        {
            var marksDetails = _context.MarksDetail
                .Include(m => m.Registration)     // To load the Registration (User)
                .Include(m => m.Subject)          // To load the Subject
                .Include(m => m.Grade)            // To load the Grade
                .Include(m => m.Class)            // To load the Class
                .Select(m => new MarksViewModel
                {
                    Id = m.Id,
                    UserID = m.UserID,
                    Registration = m.Registration,  // Ensure Registration (Full Name) is loaded
                    SubjectID = m.SubjectID,
                    Subject = m.Subject,            // Ensure the Subject is loaded
                    GradeId = m.GradeId,
                    ClassId = m.ClassId,
                    Term = m.Term,
                    Marks = m.Marks.ToString(),
                    Grade = m.Grade,                // Load the full Grade entity
                    Class = m.Class                 // Load the full Class entity
                })
                .ToList(); // Ensure this is a list of MarksViewModel objects

            // Handle case where no data is found
            if (marksDetails == null || !marksDetails.Any())
            {
                marksDetails = new List<MarksViewModel>(); // Ensure it's an empty list, not null
            }

            return View(marksDetails); // Pass the collection
        }

        public IActionResult MarksEdit(MarksViewModel model)
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MarksEdit()
        {
            var viewModel = new MarksViewModel();
            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult MarksEdit(Guid id)
        {
            // Fetch the mark details based on the provided ID
            var marks = _context.MarksDetail
                .Where(m => m.Id == id)
                .Select(m => new MarksViewModel
                {
                    Id = m.Id,
                    Marks = m.Marks,
                    GradeId = m.GradeId,
                    Grades = _context.Grades.Select(g => new SelectListItem
                    {
                        Value = g.ID.ToString(),
                        Text = $"Grade {g.Grade}"
                    }).ToList(), // Corrected closing parenthesis and added ToList()

                    ClassId = m.ClassId,
                    Classes = _context.Class.Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = $"Class {c.Class}"
                    }).ToList(), // Corrected closing parenthesis and added ToList()

                    SubjectID = m.SubjectID,
                    Subjects = _context.Subject.Select(s => new SelectListItem
                    {
                        Value = s.ID.ToString(),
                        Text = $"{s.Subject}" // Fixed display text
                    }).ToList() // Added ToList()
                }).FirstOrDefault(); // Ensures you fetch a single record or null if not found

            if (marks == null) // Fixed variable name from "subject" to "marks"
            {
                TempData["ErrorMessage"] = "Marks not found.";
                return RedirectToAction("MarksList"); // Redirect to a relevant action (e.g., "MarksList")
            }

            return View(marks);
        }

    }



}
