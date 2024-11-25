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

        [HttpGet]
        public IActionResult ViewSubject()
        {
            var model = new SubjectViewModel
            {
                // Populate the Grades dropdown list
                Grades = _context.Grades
                    .Select(g => new SelectListItem
                    {
                        Value = g.ID.ToString(),   // Grade ID as GUID (converted to string)
                        Text = $"Grade {g.Grade}"   // Display text, for example, "Grade 1"
                    })
                    .ToList(),

                // Populate the list of subjects (with related grade)
                Subjects = _context.Subject
                    .Include(s => s.Grades)  // Include Grade for each subject
                    .ToList()
            };

            return View(model);  // Pass the populated model to the view
        }

        [HttpPost]
        public IActionResult ViewSubject(SubjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Create a new subject
                    var subject = new SubjectModel
                    {
                        ID = Guid.NewGuid(),   // Generate a new GUID for the subject
                        Subject = model.Subject,   // Set the subject name
                        GradeId = model.GradeId  // Assign the selected grade
                    };

                    _context.Subject.Add(subject);  // Add subject to the database
                    _context.SaveChanges();  // Save changes to the database

                    TempData["SuccessMessage"] = "Subject added successfully!";  // Success message
                    return RedirectToAction("Subject");  // Redirect to the same action to refresh the form and list
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    TempData["ErrorMessage"] = "An error occurred while saving the subject.";  // Error handling
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Please fill all required fields correctly.";  // Validation error
            }

            // Repopulate the dropdown in case of validation errors
            model.Grades = _context.Grades
                .Select(g => new SelectListItem
                {
                    Value = g.ID.ToString(),
                    Text = $"Grade {g.Grade}"
                })
                .ToList();

            // Repopulate the subjects list
            model.Subjects = _context.Subject
                .Include(s => s.Grades)
                .ToList();

            return View(model);  // Return the view with the model to show validation errors and current data
        }

        [HttpGet]
        public IActionResult SubjectEdit(Guid id)
        {
            var subject = _context.Subject
                .Where(s => s.ID == id)
                .Select(s => new SubjectViewModel
                {
                    Id = s.ID,
                    Subject = s.Subject,
                    GradeId = s.GradeId,
                    Grades = _context.Grades.Select(g => new SelectListItem
                    {
                        Value = g.ID.ToString(),
                        Text = $"Grade {g.Grade}"
                    }).ToList()
                })
                .FirstOrDefault();

            if (subject == null)
            {
                TempData["ErrorMessage"] = "Subject not found.";
                return RedirectToAction("Index");
            }

            return View(subject);
        }


        [HttpPost]
		public IActionResult SubjectEdit(SubjectViewModel model)
		{
			if (ModelState.IsValid)
			{
				// Retrieve the existing subject from the database
				var existingSubject = _context.Subject.FirstOrDefault(s => s.ID == model.Id);

				if (existingSubject == null)
				{
					TempData["ErrorMessage"] = "Subject not found.";
					return RedirectToAction("SubjectEdit"); // Redirect to the list page if not found
				}

				// Update the subject's properties
				existingSubject.Subject = model.Subject;
				existingSubject.GradeId = model.GradeId;

				_context.Subject.Update(existingSubject);
				_context.SaveChanges();

				TempData["SuccessMessage"] = "Subject updated successfully!";
				return RedirectToAction("SubjectEdit"); // Redirect to the list page after successful update
			}

			// Reload the Grades list in case of validation errors
			model.Grades = _context.Grades.Select(g => new SelectListItem
			{
				Value = g.ID.ToString(),
				Text = $"Grade {g.Grade}"
			}).ToList();

			return View(model); // Return to the form with the validation errors
		}

	}
}