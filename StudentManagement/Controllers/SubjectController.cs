using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentManagement.Models;
using StudentManagement.ViewModel;

namespace StudentManagement.Controllers
{
    public class SubjectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Display the Subject form with a dropdown for grades
        public IActionResult Subject()
        {
            var gradeItems = _context.Grades
                .Select(g => new SelectListItem
                {
                    Value = g.ID.ToString(), // Convert GUID to string for dropdown
                    Text = $"Grade {g.Grade}" // Display Grade (e.g., "Grade 1")
                })
                .ToList();

            var viewModel = new SubjectViewModel
            {
                Grades = gradeItems
            };

            return View(viewModel);
        }

        // POST: Add a new Subject
        [HttpPost]
        public IActionResult Subject(SubjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var subject = new SubjectModel
                    {
                        ID = Guid.NewGuid(),
                        Subject = model.Subject,
                        GradeId = model.GradeId // Assign the selected grade
                    };

                    _context.Subject.Add(subject);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Subject added successfully!";
                    return RedirectToAction("Subject");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    TempData["ErrorMessage"] = "An error occurred while saving the subject.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Please fill all required fields correctly.";
            }

            // Repopulate the dropdown in case of validation errors
            model.Grades = _context.Grades
                .Select(g => new SelectListItem
                {
                    Value = g.ID.ToString(),
                    Text = $"Grade {g.Grade}"
                })
                .ToList();

            return View(model);
        }
    }
}
