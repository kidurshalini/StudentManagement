using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using StudentManagement.ViewModel;
using System;
using System.Linq;
using System.Text;
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


        [HttpPost]
        public IActionResult MarksFormSubmit(MarksViewModel model)
        {
            if (model == null || model.SubjectMarks == null || !model.SubjectMarks.Any())
            {
                TempData["ErrorMessage"] = "No marks submitted.";
                return RedirectToAction("MarksForm");
            }

            var existingUser = _context.Marks
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
                    Marks = entry.Value
                };

                _context.Marks.Add(marksDetails);
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Marks submitted successfully!";
            return RedirectToAction("MarksForm");
        }


    }



}
