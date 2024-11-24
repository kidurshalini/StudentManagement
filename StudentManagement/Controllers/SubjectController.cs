using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet]
        public IActionResult Subject()
        {
            var gradeItems = _context.Grades
        .Select(g => new SelectListItem
        {
            Value = g.ID.ToString(),
            Text = $"Grade {g.Grade}"
        })
        .ToList();

            var viewModel = new SubjectViewModel
            {
                Grades = gradeItems
            };

            return View(viewModel);


        }
        [HttpPost]
        public IActionResult Subject(SubjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var Subject = new SubjectModel
                    {
                        ID = Guid.NewGuid(),
                        Subject = model.Subject,
                        GradeId = model.GradeId // Assign the selected grade
                    };

                    _context.Subject.Add(Subject);
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



        public IActionResult Class()
        {

            var gradeItems = _context.Grades
                .Select(g => new SelectListItem
                {
                    Value = g.ID.ToString(),
                    Text = $"Grade {g.Grade}"
                })
                .ToList();

            var viewModel = new ClassViewModel
            {
                Grades = gradeItems
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Class(ClassViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var Class = new ClassModel
                    {
                        ID = Guid.NewGuid(),
                        Class = model.Class,
                        GradeId = model.GradeId // Assign the selected grade
                    };

                    _context.Class.Add(Class);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Class added successfully!";
                    return RedirectToAction("Class");
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