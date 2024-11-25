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
        //[HttpPost]
        //public IActionResult Subject(SubjectViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var Subject = new SubjectModel
        //            {
        //                ID = Guid.NewGuid(),
        //                Subject = model.Subject,
        //                GradeId = model.GradeId // Assign the selected grade
        //            };

        //            _context.Subject.Add(Subject);
        //            _context.SaveChanges();

        //            TempData["SuccessMessage"] = "Subject added successfully!";
        //            return RedirectToAction("Subject");
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Error: {ex.Message}");
        //            TempData["ErrorMessage"] = "An error occurred while saving the subject.";
        //        }
        //    }
        //    else
        //    {
        //        TempData["ErrorMessage"] = "Please fill all required fields correctly.";
        //    }

        //    // Repopulate the dropdown in case of validation errors
        //    model.Grades = _context.Grades
        //        .Select(g => new SelectListItem
        //        {
        //            Value = g.ID.ToString(),
        //            Text = $"Grade {g.Grade}"
        //        })
        //        .ToList();

        //    return View(model);
        //}

        [HttpPost]
        public IActionResult Subject(SubjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if the combination of GradeId and Subject already exists
                    var existingSubject = _context.Subject
                        .FirstOrDefault(s => s.GradeId == model.GradeId && s.Subject == model.Subject);

                    if (existingSubject != null)
                    {
                        TempData["ErrorMessage"] = "This subject for the selected grade has already been entered.";
                        // Repopulate the dropdown list in case of error
                        model.Grades = _context.Grades
                            .Select(g => new SelectListItem
                            {
                                Value = g.ID.ToString(),
                                Text = $"Grade {g.Grade}"
                            })
                            .ToList();

                        return View(model); // Return to the same view with the error message
                    }

                    // Proceed with adding the new subject if no duplicates found
                    var newSubject = new SubjectModel
                    {
                        ID = Guid.NewGuid(),
                        Subject = model.Subject,
                        GradeId = model.GradeId // Assign the selected grade
                    };

                    _context.Subject.Add(newSubject);
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


                    var existingClass = _context.Class
                     .FirstOrDefault(c => c.GradeId == model.GradeId && c.Class == model.Class);


                    if (existingClass != null)
                    {
                        TempData["ErrorMessage"] = "This subject for the selected grade has already been entered.";
                        // Repopulate the dropdown list in case of error
                        model.Grades = _context.Grades
                            .Select(g => new SelectListItem
                            {
                                Value = g.ID.ToString(),
                                Text = $"Grade {g.Grade}"
                            })
                            .ToList();

                        return View(model); // Return to the same view with the error message
                    }

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

        [HttpGet]
        public IActionResult ClassView()
        {
            var classDetails = _context.Class
                .Include(c => c.Grades) // Ensure Grades navigation property is loaded
                .Select(c => new ClassViewModel
                {
                    Id = c.ID,
                    GradeId = c.GradeId,                  // Use GradeId for reference
                    Class = $"Class {c.Class}",                    
                    Grades = new List<SelectListItem>    // Use Grade integer for display
                    {
                new SelectListItem
                {
                    Text = $"Grade {c.Grades.Grade}", // Display Grade as "Grade X"
                    Value = c.GradeId.ToString()     // GradeId as value
                }
                    }
                })
                .ToList();

            return View(classDetails);
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
                return RedirectToAction("SubjectEdit");
            }

            return View(subject);
        }


        [HttpPost]
		public IActionResult SubjectEdit(SubjectViewModel model)
		{
			if (ModelState.IsValid)
			{
                // Check if the combination of GradeId and Subject already exists
                var existingSubjects = _context.Subject
                    .FirstOrDefault(s => s.GradeId == model.GradeId && s.Subject == model.Subject);

                if (existingSubjects != null)
                {
                    TempData["ErrorMessage"] = "This subject for the selected grade has already been entered.";
                    // Repopulate the dropdown list in case of error
                    model.Grades = _context.Grades
                        .Select(g => new SelectListItem
                        {
                            Value = g.ID.ToString(),
                            Text = $"Grade {g.Grade}"
                        })
                        .ToList();

                    return View(model); // Return to the same view with the error message
                }
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

			
				return RedirectToAction("ViewSubject"); // Redirect to the list page after successful update
			}

			// Reload the Grades list in case of validation errors
			model.Grades = _context.Grades.Select(g => new SelectListItem
			{
				Value = g.ID.ToString(),
				Text = $"Grade {g.Grade}"
			}).ToList();

			return View(model); // Return to the form with the validation errors
		}

        [HttpGet]
        public IActionResult SubjectDelete(Guid id)
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
                return RedirectToAction("SubjectDelete");
            }

            return View(subject);
        }

        [HttpPost]
        public IActionResult SubjectDelete(SubjectViewModel model)
        {
            if (ModelState.IsValid)
            {
             
            
                // Retrieve the existing subject from the database
                var existingSubject = _context.Subject.FirstOrDefault(s => s.ID == model.Id);

          
                _context.Subject.Remove(existingSubject);
                _context.SaveChanges();

             
                return RedirectToAction("ViewSubject"); 
            }

            // Reload the Grades list in case of validation errors
            model.Grades = _context.Grades.Select(g => new SelectListItem
            {
                Value = g.ID.ToString(),
                Text = $"Grade {g.Grade}"
            }).ToList();

            return View(model); // Return to the form with the validation errors
        }

        // GET: ClassEdit
        [HttpGet]
        public IActionResult ClassEdit(Guid Id)
        {
            // Retrieve the class by ID and populate the view model
            var classViewModel = _context.Class
                .Where(s => s.ID == Id)
                .Select(s => new ClassViewModel
                {
                    Id = s.ID,
                    Class = s.Class,
                    GradeId = s.GradeId,
                    Grades = _context.Grades
                        .Select(g => new SelectListItem
                        {
                            Value = g.ID.ToString(),
                            Text = $"Grade {g.Grade}"
                        })
                        .ToList()
                })
                .FirstOrDefault();

            if (classViewModel == null)
            {
                TempData["ErrorMessage"] = "Class not found.";
                return RedirectToAction("ClassEdit"); // Redirect to the list view
            }

            return View(classViewModel); // Pass the model to the view
        }

        // POST: ClassEdit
        [HttpPost]
        public IActionResult ClassEdit(ClassViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if a class with the same GradeId and Class name already exists
                var duplicateClass = _context.Class
                    .FirstOrDefault(c => c.GradeId == model.GradeId && c.Class == model.Class && c.ID != model.Id);

                if (duplicateClass != null)
                {
                    TempData["ErrorMessage"] = "A class with the same grade and name already exists.";
                    // Repopulate the dropdown list
                    model.Grades = _context.Grades
                        .Select(g => new SelectListItem
                        {
                            Value = g.ID.ToString(),
                            Text = $"Grade {g.Grade}"
                        })
                        .ToList();

                    return View(model); // Return to the view with an error message
                }

                // Retrieve the existing class record
                var existingClass = _context.Class.FirstOrDefault(c => c.ID == model.Id);

                if (existingClass == null)
                {
                    TempData["ErrorMessage"] = "Class not found.";
                    return RedirectToAction("ClassEdit"); // Redirect to the list page if not found
                }

                // Update the properties
                existingClass.Class = model.Class;
                existingClass.GradeId = model.GradeId;

                _context.Class.Update(existingClass);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Class updated successfully.";
                return RedirectToAction("ClassView"); // Redirect to the list page after successful update
            }

            // Reload the Grades dropdown if the model state is invalid
            model.Grades = _context.Grades
                .Select(g => new SelectListItem
                {
                    Value = g.ID.ToString(),
                    Text = $"Grade {g.Grade}"
                })
                .ToList();

            return View(model); // Return the form with validation errors
        }

    }
}